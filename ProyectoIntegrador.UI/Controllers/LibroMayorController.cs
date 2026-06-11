using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using Microsoft.AspNetCore.WebUtilities;
using ProyectoIntegrador.UI.Services;


namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class LibroMayorController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly ILibroMayorExcelService _excelService;
    private readonly ILibroMayorPdfService _pdfService;

    public LibroMayorController(
        ApiClient apiClient,
        ILibroMayorExcelService excelService,
        ILibroMayorPdfService pdfService)
    {
        _apiClient = apiClient;
        _excelService = excelService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid clienteId,
        List<Guid>? cuentaIds,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        Guid? ejercicioId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=200");

        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{clienteId}/cuentas-imputables");

        var url = ConstruirUrlLibroMayor(clienteId, cuentaIds, fechaDesde, fechaHasta, ejercicioId);
        var mayorResponse = await _apiClient.GetAsync<LibroMayorResponseViewModel>(url);

        if (mayorResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!mayorResponse.EsExitoso || mayorResponse.Data is null)
        {
            TempData["Error"] = mayorResponse.MensajeError ?? "No se pudo cargar el libro mayor.";
            return RedirectToAction("Index", "Clientes");
        }

        var vm = new LibroMayorViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            EjercicioId = ejercicioId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            CuentaIds = cuentaIds?.ToList() ?? new List<Guid>(),
            Ejercicios = ejerciciosResponse.Data?.Datos ?? new List<EjercicioContableViewModel>(),
            Cuentas = cuentasResponse.Data ?? new List<CuentaContableViewModel>(),
            CuentasMayor = mayorResponse.Data.Cuentas
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Exportar(
        Guid clienteId,
        List<Guid>? cuentaIds,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        Guid? ejercicioId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var url = ConstruirUrlLibroMayor(clienteId, cuentaIds, fechaDesde, fechaHasta, ejercicioId);
        var mayorResponse = await _apiClient.GetAsync<LibroMayorResponseViewModel>(url);

        if (mayorResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!mayorResponse.EsExitoso || mayorResponse.Data is null)
        {
            TempData["Error"] = mayorResponse.MensajeError ?? "No se pudo exportar el libro mayor.";
            return RedirectToAction("Index", new { clienteId });
        }

        var vm = new LibroMayorViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty,
            EjercicioId = ejercicioId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            CuentaIds = cuentaIds?.ToList() ?? new List<Guid>(),
            Ejercicios = new List<EjercicioContableViewModel>(),
            Cuentas = new List<CuentaContableViewModel>(),
            CuentasMayor = mayorResponse.Data.Cuentas
        };

        var bytes = _excelService.Generar(vm);

        var nombreArchivo = $"LibroMayor_{clienteResponse.Data?.RazonSocial}_{DateTime.Today:yyyyMMdd}.xlsx"
            .Replace(" ", "_");

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombreArchivo);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(
    Guid clienteId,
    List<Guid>? cuentaIds,
    DateOnly? fechaDesde,
    DateOnly? fechaHasta,
    Guid? ejercicioId)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var url = ConstruirUrlLibroMayor(clienteId, cuentaIds, fechaDesde, fechaHasta, ejercicioId);
        var mayorResponse = await _apiClient.GetAsync<LibroMayorResponseViewModel>(url);

        if (mayorResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!mayorResponse.EsExitoso || mayorResponse.Data is null)
        {
            TempData["Error"] = mayorResponse.MensajeError ?? "No se pudo exportar el libro mayor.";
            return RedirectToAction("Index", new { clienteId });
        }

        var vm = new LibroMayorViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty,
            EjercicioId = ejercicioId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            CuentaIds = cuentaIds?.ToList() ?? new List<Guid>(),
            Ejercicios = new List<EjercicioContableViewModel>(),
            Cuentas = new List<CuentaContableViewModel>(),
            CuentasMayor = mayorResponse.Data.Cuentas
        };

        var bytes = _pdfService.Generar(vm);

        var nombreArchivo = $"LibroMayor_{clienteResponse.Data?.RazonSocial}_{DateTime.Today:yyyyMMdd}.pdf"
            .Replace(" ", "_");

        return File(bytes, "application/pdf", nombreArchivo);
    }

    private static string ConstruirUrlLibroMayor(
        Guid clienteId,
        List<Guid>? cuentaIds,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        Guid? ejercicioId)
    {
        var query = new Dictionary<string, string?>
        {
            ["clienteId"] = clienteId.ToString(),
            ["fechaDesde"] = fechaDesde?.ToString("yyyy-MM-dd"),
            ["fechaHasta"] = fechaHasta?.ToString("yyyy-MM-dd"),
            ["ejercicioId"] = ejercicioId?.ToString()
        };

        var url = QueryHelpers.AddQueryString("api/libro-mayor", query!);

        if (cuentaIds is { Count: > 0 })
        {
            foreach (var id in cuentaIds)
                url = QueryHelpers.AddQueryString(url, "cuentaIds", id.ToString());
        }

        return url;
    }
}