using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers
{
    [Authorize]
    public class EstadoResultadosController : Controller
    {
        private readonly ApiClient _apiClient;

        public EstadoResultadosController(ApiClient apiClient) => _apiClient = apiClient;

        [HttpGet]
        public async Task<IActionResult> Index(Guid clienteId, DateOnly? fechaDesde, DateOnly? fechaHasta)
        {
            var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
            if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
            if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
            {
                TempData["Error"] = "No se pudo cargar el cliente.";
                return RedirectToAction("Index", "Clientes");
            }
            var vm = new EstadoResultadosViewModel
            {
                ClienteId = clienteId,
                ClienteNombre = clienteResponse.Data.RazonSocial,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };
            if (fechaDesde.HasValue && fechaHasta.HasValue)
            {
                var response = await _apiClient.GetAsync<EstadoResultadosResponseViewModel>(
                    $"api/estado-resultados" +
                    $"?clienteId={clienteId}" +
                    $"&fechaDesde={fechaDesde:yyyy-MM-dd}" +
                    $"&fechaHasta={fechaHasta:yyyy-MM-dd}");
                if (response.EsNoAutorizado)
                {
                    return RedirectToAction("Login", "Auth");
                }
                if (!response.EsExitoso || response.Data is null)
                {
                    TempData["Error"] = response.MensajeError ?? "No se pudo generar el estado de resultados.";
                    return View(vm);
                }
                vm.Resultado = response.Data;
            }

            return View(vm);
        }
    }
}
