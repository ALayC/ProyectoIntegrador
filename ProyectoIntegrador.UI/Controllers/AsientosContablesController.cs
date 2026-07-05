using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class AsientosContablesController : Controller
{
    private readonly ApiClient _apiClient;

    public AsientosContablesController(ApiClient apiClient) => _apiClient = apiClient;

    [HttpGet]
    public async Task<IActionResult> Crear(Guid clienteId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=100");

        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{clienteId}/cuentas-imputables");

        var vm = new CrearAsientoViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Ejercicios = ejerciciosResponse.Data?.Datos ?? new(),
            Cuentas = cuentasResponse.Data ?? new()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Guid clienteId, CrearAsientoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RecargarSelectsAsync(model);
            return View(model);
        }

        var totalDebe  = model.Lineas.Sum(l => l.Debe  * l.TipoCambio);
        var totalHaber = model.Lineas.Sum(l => l.Haber * l.TipoCambio);

        if (Math.Abs(totalDebe - totalHaber) > 0.001m)
        {
            ModelState.AddModelError(string.Empty,
                $"El asiento está desbalanceado en moneda base: Debe={totalDebe:N2}, Haber={totalHaber:N2}. Deben ser iguales.");
            await RecargarSelectsAsync(model);
            return View(model);
        }

        var response = await _apiClient.PostAsync<AsientoContableViewModel>("api/asientos-contables", new
        {
            clienteId,
            ejercicioId = model.EjercicioId,
            fecha = model.Fecha,
            glosa = model.Glosa,
            lineas = model.Lineas.Select(l => new
            {
                cuentaContableId = l.CuentaContableId,
                centroCostoId = l.CentroCostoId,
                debe = l.Debe,
                haber = l.Haber,
                moneda = l.Moneda,
                tipoCambio = l.TipoCambio
            })
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo registrar el asiento.");
            await RecargarSelectsAsync(model);
            return View(model);
        }

        TempData["Exito"] = $"Asiento N° {response.Data!.Numero} registrado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId });
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid clienteId,
        Guid? ejercicioId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int pagina = 1,
        int cantidadPorPagina = 20)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=100");

        var url = $"api/asientos-contables?clienteId={clienteId}&pagina={pagina}&cantidadPorPagina={cantidadPorPagina}";
        if (ejercicioId.HasValue) url += $"&ejercicioId={ejercicioId}";
        if (fechaDesde.HasValue) url += $"&fechaDesde={fechaDesde:yyyy-MM-dd}";
        if (fechaHasta.HasValue) url += $"&fechaHasta={fechaHasta:yyyy-MM-dd}";

        var response = await _apiClient.GetAsync<LibroDiarioPaginadoResponse>(url);

        var vm = new LibroDiarioIndexViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            EjercicioId = ejercicioId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = pagina,
            CantidadPorPagina = cantidadPorPagina,
            Ejercicios = ejerciciosResponse.Data?.Datos ?? new(),
            Items = response.Data?.Items ?? new(),
            Total = response.Data?.Total ?? 0
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Detalles(Guid id, Guid clienteId)
    {
        var asientoResponse = await _apiClient.GetAsync<AsientoContableViewModel>($"api/asientos-contables/{id}");
        if (asientoResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!asientoResponse.EsExitoso || asientoResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el asiento contable.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        var ejercicioResponse = await _apiClient.GetAsync<EjercicioContableViewModel>(
            $"api/ejercicios/{asientoResponse.Data.EjercicioId}");

        if (clienteResponse.EsNoAutorizado || ejercicioResponse.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        ViewBag.ClienteId = clienteId;
        ViewBag.ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty;
        ViewBag.EjercicioDescripcion = ejercicioResponse.EsExitoso && ejercicioResponse.Data is not null
            ? $"{ejercicioResponse.Data.FechaInicio:dd/MM/yyyy} – {ejercicioResponse.Data.FechaFin:dd/MM/yyyy} ({ejercicioResponse.Data.Estado})"
            : string.Empty;

        return View(asientoResponse.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revertir(Guid id, Guid clienteId)
    {
        var response = await _apiClient.PostAsync<AsientoContableViewModel>(
            $"api/asientos-contables/{id}/revertir",
            new { });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo revertir el asiento.";
            return RedirectToAction(nameof(Detalles), new { id, clienteId });
        }

        TempData["Exito"] = $"Asiento N° {response.Data!.Numero} (reversión) creado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId });
    }

    // ── Proxy para tipo de cambio (evita CORS del browser al API) ──────────
    [HttpGet]
    public async Task<IActionResult> CotizacionUsd([FromQuery] string fecha)
    {
        // Determinar fecha a consultar
        DateOnly fechaConsulta;
        if (!DateOnly.TryParse(fecha, out fechaConsulta))
            fechaConsulta = DateOnly.FromDateTime(DateTime.Today);

        var response = await _apiClient.GetAsync<CotizacionUsdViewModel>(
            $"api/tipocambio?moneda=USD&fecha={fechaConsulta:yyyy-MM-dd}");

        if (!response.EsExitoso || response.Data is null)
            return Json(new { ok = false, esFallback = false });

        // Detectar si es fallback comparando la fecha retornada
        var esFallback = response.Data.Fecha != fechaConsulta;

        return Json(new
        {
            ok = true,
            valor = response.Data.Valor,
            fechaReal = response.Data.Fecha.ToString("dd/MM/yyyy"),
            esFallback
        });
    }

    private class CotizacionUsdViewModel
    {
        public string Moneda { get; set; } = string.Empty;
        public DateOnly Fecha { get; set; }
        public decimal Valor { get; set; }
        public string Fuente { get; set; } = string.Empty;
    }

    // ──────────────────────────────────────────────
    private async Task RecargarSelectsAsync(CrearAsientoViewModel model)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{model.ClienteId}");
        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={model.ClienteId}&pagina=1&cantidadPorPagina=100");
        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{model.ClienteId}/cuentas-imputables");

        model.ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty;
        model.Ejercicios = ejerciciosResponse.Data?.Datos ?? new();
        model.Cuentas = cuentasResponse.Data ?? new();
    }

    // DTO local para deserializar la respuesta paginada del API
    private class LibroDiarioPaginadoResponse
    {
        public List<AsientoContableResumenViewModel> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}