using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class ImportacionController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IImportacionExcelService _excelService;

    private const string SessionKey = "ImportacionParsed";
    private const string SessionEjercicioKey = "ImportacionEjercicioId";
    private const string SessionClienteKey = "ImportacionClienteId";

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ImportacionController(ApiClient apiClient, IImportacionExcelService excelService)
    {
        _apiClient = apiClient;
        _excelService = excelService;
    }

    // ── Paso 1: Mostrar formulario de carga ────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Iniciar(Guid clienteId)
    {
        var (cliente, ejercicios) = await CargarClienteYEjerciciosAsync(clienteId);
        if (cliente is null) return RedirectToAction("Index", "Clientes");

        var vm = new ImportacionIniciarViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = cliente.RazonSocial,
            Ejercicios = ejercicios
        };

        return View(vm);
    }

    // ── Descarga del template personalizado ───────────────────────────────

    [HttpGet]
    public async Task<IActionResult> DescargarTemplate(Guid clienteId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{clienteId}/cuentas-imputables");

        if (!cuentasResponse.EsExitoso || cuentasResponse.Data is null)
        {
            TempData["Error"] = "No se pudieron cargar las cuentas del cliente.";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        var nombre = clienteResponse.Data?.RazonSocial ?? "Cliente";
        var bytes = _excelService.GenerarTemplate(nombre, cuentasResponse.Data);

        var nombreArchivo = $"template_asientos_{nombre.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }

    // ── Paso 2: Parsear el Excel subido ───────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Parsear(Guid clienteId, Guid ejercicioId, IFormFile archivo)
    {
        var (cliente, ejercicios) = await CargarClienteYEjerciciosAsync(clienteId);
        if (cliente is null) return RedirectToAction("Index", "Clientes");

        // Validaciones básicas del archivo
        if (archivo is null || archivo.Length == 0)
        {
            TempData["Error"] = "Debe seleccionar un archivo Excel.";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        if (!archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "El archivo debe tener formato .xlsx.";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        if (ejercicioId == Guid.Empty)
        {
            TempData["Error"] = "Debe seleccionar un ejercicio contable.";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        // Cargar cuentas del cliente para resolver códigos
        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{clienteId}/cuentas-imputables");

        if (!cuentasResponse.EsExitoso || cuentasResponse.Data is null)
        {
            TempData["Error"] = "No se pudieron cargar las cuentas del cliente.";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        var cuentasPorCodigo = cuentasResponse.Data
            .ToDictionary(c => c.Codigo, c => c, StringComparer.OrdinalIgnoreCase);

        // Parsear
        List<AsientoImportacionViewModel> asientos;
        try
        {
            using var stream = archivo.OpenReadStream();
            asientos = _excelService.Parsear(stream, cuentasPorCodigo);
        }
        catch (ImportacionFormatoException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }
        catch (Exception)
        {
            TempData["Error"] = "El archivo no pudo ser leído. Verifique que sea un Excel válido (.xlsx).";
            return RedirectToAction(nameof(Iniciar), new { clienteId });
        }

        // Serializar los asientos válidos en Session para la confirmación
        var sesionData = asientos
            .Where(a => a.EsValido)
            .Select(a => new AsientoParseadoSession
            {
                NumAsiento = a.NumAsiento,
                Fecha = a.Fecha.ToString("yyyy-MM-dd"),
                Glosa = a.Glosa,
                EsValido = true,
                Lineas = a.Lineas.Select(l => new LineaParseadaSession
                {
                    CuentaContableId = l.CuentaContableId!.Value,
                    Debe = l.Debe,
                    Haber = l.Haber,
                    Moneda = l.Moneda,
                    TipoCambio = l.TipoCambio
                }).ToList()
            }).ToList();

        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(sesionData));
        HttpContext.Session.SetString(SessionEjercicioKey, ejercicioId.ToString());
        HttpContext.Session.SetString(SessionClienteKey, clienteId.ToString());

        // Preparar vista previa con todos los asientos (válidos e inválidos)
        var ejercicioResponse = await _apiClient.GetAsync<EjercicioContableViewModel>(
            $"api/ejercicios/{ejercicioId}");

        var ejercicioDesc = ejercicioResponse.EsExitoso && ejercicioResponse.Data is not null
            ? $"{ejercicioResponse.Data.FechaInicio:dd/MM/yyyy} – {ejercicioResponse.Data.FechaFin:dd/MM/yyyy} ({ejercicioResponse.Data.Estado})"
            : ejercicioId.ToString();

        var vm = new ImportacionVistaPreviaViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = cliente.RazonSocial,
            EjercicioId = ejercicioId,
            EjercicioDescripcion = ejercicioDesc,
            Asientos = asientos
        };

        return View("VistaPrevia", vm);
    }

    // ── Paso 3: Confirmar selección del usuario ───────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(List<int> seleccionados)
    {
        // Leer datos de sesión
        var json = HttpContext.Session.GetString(SessionKey);
        var ejercicioStr = HttpContext.Session.GetString(SessionEjercicioKey);
        var clienteStr = HttpContext.Session.GetString(SessionClienteKey);

        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(ejercicioStr) || string.IsNullOrEmpty(clienteStr))
        {
            TempData["Error"] = "La sesión expiró. Por favor, vuelva a subir el archivo.";
            return RedirectToAction("Index", "Clientes");
        }

        var clienteId = Guid.Parse(clienteStr);
        var ejercicioId = Guid.Parse(ejercicioStr);

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var todosLosAsientos = JsonSerializer.Deserialize<List<AsientoParseadoSession>>(json, JsonOpts) ?? new();

        // Filtrar solo los que el usuario seleccionó
        var seleccionSet = new HashSet<int>(seleccionados);
        var asientosSeleccionados = todosLosAsientos
            .Where(a => seleccionSet.Contains(a.NumAsiento))
            .ToList();

        var rechazados = todosLosAsientos
            .Where(a => !seleccionSet.Contains(a.NumAsiento))
            .Select(a => a.NumAsiento)
            .ToList();

        // Construir el request bulk
        var bulkRequest = new
        {
            clienteId,
            ejercicioId,
            asientos = asientosSeleccionados.Select(a => new
            {
                numAsiento = a.NumAsiento,
                fecha = a.Fecha,
                glosa = a.Glosa,
                lineas = a.Lineas.Select(l => new
                {
                    cuentaContableId = l.CuentaContableId,
                    debe = l.Debe,
                    haber = l.Haber,
                    moneda = l.Moneda,
                    tipoCambio = l.TipoCambio
                })
            })
        };

        // Limpiar la sesión
        HttpContext.Session.Remove(SessionKey);
        HttpContext.Session.Remove(SessionEjercicioKey);
        HttpContext.Session.Remove(SessionClienteKey);

        var resultadoVm = new ResultadoImportacionViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty,
            TotalRechazados = rechazados.Count
        };

        if (asientosSeleccionados.Count > 0)
        {
            var response = await _apiClient.PostAsync<ResultadoBulkApiResponse>(
                "api/asientos-contables/importar", bulkRequest);

            if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

            if (!response.EsExitoso || response.Data is null)
            {
                TempData["Error"] = response.MensajeError ?? "Error al importar los asientos.";
                return RedirectToAction(nameof(Iniciar), new { clienteId });
            }

            resultadoVm.TotalEnviados = response.Data.TotalEnviados;
            resultadoVm.TotalCreados = response.Data.TotalCreados;
            resultadoVm.TotalErrores = response.Data.TotalErrores;

            foreach (var r in response.Data.Resultados)
            {
                resultadoVm.Resultados.Add(new ResultadoAsientoViewModel
                {
                    NumAsiento = r.NumAsiento,
                    Estado = r.Exitoso ? "Creado" : "Error",
                    NumeroAsientoGenerado = r.NumeroAsientoGenerado,
                    MensajeError = r.MensajeError
                });
            }
        }

        // Agregar rechazados al resultado
        foreach (var numR in rechazados)
        {
            resultadoVm.Resultados.Add(new ResultadoAsientoViewModel
            {
                NumAsiento = numR,
                Estado = "Rechazado"
            });
        }

        resultadoVm.Resultados = resultadoVm.Resultados.OrderBy(r => r.NumAsiento).ToList();

        return View("Resultado", resultadoVm);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<(ClienteListViewModel? cliente, List<EjercicioContableViewModel> ejercicios)>
        CargarClienteYEjerciciosAsync(Guid clienteId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado || !clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return (null, new());
        }

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=100");

        return (clienteResponse.Data, ejerciciosResponse.Data?.Datos ?? new());
    }

    // DTO local para deserializar la respuesta del API bulk
    private sealed class ResultadoBulkApiResponse
    {
        public int TotalEnviados { get; set; }
        public int TotalCreados { get; set; }
        public int TotalErrores { get; set; }
        public List<ResultadoAsientoApiItem> Resultados { get; set; } = new();
    }

    private sealed class ResultadoAsientoApiItem
    {
        public int NumAsiento { get; set; }
        public bool Exitoso { get; set; }
        public int? NumeroAsientoGenerado { get; set; }
        public string? MensajeError { get; set; }
    }
}
