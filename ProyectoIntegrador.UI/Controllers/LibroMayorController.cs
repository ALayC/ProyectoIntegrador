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

    public LibroMayorController(ApiClient apiClient)
    {
        _apiClient = apiClient;
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
            {
                url = QueryHelpers.AddQueryString(url, "cuentaIds", id.ToString());
            }
        }

        return url;
    }

    private class LibroMayorResponseViewModel
    {
        public Guid ClienteId { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public Guid? EjercicioId { get; set; }
        public List<LibroMayorCuentaViewModel> Cuentas { get; set; } = new();
    }
}
