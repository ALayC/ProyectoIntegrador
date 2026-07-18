using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class EjerciciosController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IReporteCierreExcelService _excelService;
    private readonly IReporteCierrePdfService _pdfService;

    public EjerciciosController(ApiClient apiClient, IReporteCierreExcelService excelService, IReporteCierrePdfService pdfService)
    {
        _apiClient = apiClient;
        _excelService = excelService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid clienteId, int pagina = 1, int cantidadPorPagina = 10)
    {
        var response = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina={pagina}&cantidadPorPagina={cantidadPorPagina}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = clienteResponse.MensajeError ?? "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudieron cargar los ejercicios contables.";
            return View(new EjercicioContableIndexViewModel
            {
                ClienteId = clienteId,
                ClienteNombre = clienteResponse.Data.RazonSocial,
                Paginado = new PaginadoViewModel<EjercicioContableViewModel>()
            });
        }

        var paginado = response.Data ?? new PaginadoViewModel<EjercicioContableViewModel>();
        var tieneAbierto = paginado.Datos.Any(e => e.Estado == "Abierto");

        return View(new EjercicioContableIndexViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Paginado = paginado,
            TieneEjercicioAbierto = tieneAbierto
        });
    }

    [HttpGet]
    public IActionResult Crear(Guid clienteId)
    {
        return View(new EjercicioContableFormViewModel { ClienteId = clienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Guid clienteId, EjercicioContableFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ClienteId = clienteId;
            return View(model);
        }

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = clienteResponse.MensajeError ?? "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var response = await _apiClient.PostAsync<EjercicioContableViewModel>("api/ejercicios", new
        {
            clienteId,
            fechaInicio = model.FechaInicio,
            fechaFin = model.FechaFin
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo crear el ejercicio contable.");
            return View(model);
        }

        TempData["Exito"] = "Ejercicio contable creado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<EjercicioContableViewModel>($"api/ejercicios/{id}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Ejercicio contable no encontrado.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        if (response.Data.ClienteId != clienteId)
        {
            TempData["Error"] = "El ejercicio contable no pertenece al cliente indicado.";
            return RedirectToAction(nameof(Index), new { clienteId = response.Data.ClienteId });
        }

        return View(new EjercicioContableFormViewModel
        {
            Id = id,
            ClienteId = response.Data.ClienteId,
            FechaInicio = response.Data.FechaInicio,
            FechaFin = response.Data.FechaFin
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, Guid clienteId, EjercicioContableFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.ClienteId = clienteId;
            return View(model);
        }

        var ejercicioResponse = await _apiClient.GetAsync<EjercicioContableViewModel>($"api/ejercicios/{id}");

        if (ejercicioResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!ejercicioResponse.EsExitoso || ejercicioResponse.Data is null)
        {
            TempData["Error"] = ejercicioResponse.MensajeError ?? "Ejercicio contable no encontrado.";
            return RedirectToAction("Index", "Clientes");
        }

        if (ejercicioResponse.Data.ClienteId != clienteId)
        {
            TempData["Error"] = "El ejercicio contable no pertenece al cliente indicado.";
            return RedirectToAction(nameof(Index), new { clienteId = ejercicioResponse.Data.ClienteId });
        }

        var response = await _apiClient.PutAsync<EjercicioContableViewModel>($"api/ejercicios/{id}", new
        {
            fechaInicio = model.FechaInicio,
            fechaFin = model.FechaFin
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo actualizar el ejercicio contable.");
            return View(model);
        }

        TempData["Exito"] = "Ejercicio contable actualizado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId = ejercicioResponse.Data.ClienteId });
    }

    [HttpGet]
    public async Task<IActionResult> CierreEjercicio(Guid clienteId, Guid? ejercicioId = null)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = clienteResponse.MensajeError ?? "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=50");

        if (ejerciciosResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var ejercicios = ejerciciosResponse.EsExitoso && ejerciciosResponse.Data is not null
            ? ejerciciosResponse.Data.Datos
            : new List<EjercicioContableViewModel>();

        CierreEjercicioViewModel? resultadoCierre = null;
        if (TempData["CierreResultado"] is string json)
            resultadoCierre = System.Text.Json.JsonSerializer.Deserialize<CierreEjercicioViewModel>(json);

        // Si viene ejercicioId en query string, preseleccionamos ese
        var seleccionado = ejercicioId
            ?? (resultadoCierre is not null ? resultadoCierre.EjercicioId : (Guid?)null);

        return View(new CierreEjercicioPageViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Ejercicios = ejercicios,
            EjercicioSeleccionadoId = seleccionado,
            ResultadoCierre = resultadoCierre
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(Guid id, Guid clienteId)
    {
        var response = await _apiClient.PostAsync<CierreEjercicioViewModel>($"api/ejercicios/{id}/cerrar", new { });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo cerrar el ejercicio contable.";
            return RedirectToAction(nameof(CierreEjercicio), new { clienteId, ejercicioId = id });
        }

        if (response.Data is not null)
        {
            TempData["CierreResultado"] = System.Text.Json.JsonSerializer.Serialize(response.Data);
        }

        TempData["Exito"] = "Ejercicio cerrado correctamente.";
        return RedirectToAction(nameof(CierreEjercicio), new { clienteId });
    }

    [HttpGet]
    public async Task<IActionResult> ReporteCierre(Guid clienteId, Guid ejercicioId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var ejercicioResponse = await _apiClient.GetAsync<EjercicioContableViewModel>($"api/ejercicios/{ejercicioId}");
        if (!ejercicioResponse.EsExitoso || ejercicioResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el ejercicio.";
            return RedirectToAction(nameof(CierreEjercicio), new { clienteId });
        }

        var asientosResponse = await _apiClient.GetAsync<List<AsientoCierreViewModel>>(
            $"api/ejercicios/{ejercicioId}/asientos-cierre");

        var vm = new ReporteCierreViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Ejercicio = ejercicioResponse.Data,
            Asientos = asientosResponse.Data ?? new()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarExcelCierre(Guid clienteId, Guid ejercicioId)
    {
        var vm = await ObtenerReporteCierreVm(clienteId, ejercicioId);
        if (vm is null) return RedirectToAction(nameof(ReporteCierre), new { clienteId, ejercicioId });

        var bytes = _excelService.Generar(vm);
        var nombre = $"AsientosCierre_{vm.ClienteNombre}_{DateTime.Today:yyyyMMdd}.xlsx".Replace(" ", "_");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdfCierre(Guid clienteId, Guid ejercicioId)
    {
        var vm = await ObtenerReporteCierreVm(clienteId, ejercicioId);
        if (vm is null) return RedirectToAction(nameof(ReporteCierre), new { clienteId, ejercicioId });

        var bytes = _pdfService.Generar(vm);
        var nombre = $"AsientosCierre_{vm.ClienteNombre}_{DateTime.Today:yyyyMMdd}.pdf".Replace(" ", "_");
        return File(bytes, "application/pdf", nombre);
    }

    private async Task<ReporteCierreViewModel?> ObtenerReporteCierreVm(Guid clienteId, Guid ejercicioId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null) return null;

        var ejercicioResponse = await _apiClient.GetAsync<EjercicioContableViewModel>($"api/ejercicios/{ejercicioId}");
        if (!ejercicioResponse.EsExitoso || ejercicioResponse.Data is null) return null;

        var asientosResponse = await _apiClient.GetAsync<List<AsientoCierreViewModel>>(
            $"api/ejercicios/{ejercicioId}/asientos-cierre");

        return new ReporteCierreViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Ejercicio = ejercicioResponse.Data,
            Asientos = asientosResponse.Data ?? new()
        };
    }
}
