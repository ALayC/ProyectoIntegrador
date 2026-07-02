using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class BalanceGeneralController : Controller
{
    private readonly ApiClient _apiClient;

    public BalanceGeneralController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid clienteId, DateOnly? fechaHasta)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var vm = new BalanceGeneralViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            FechaHasta = fechaHasta
        };

        if (fechaHasta.HasValue)
        {
            var response = await _apiClient.GetAsync<BalanceGeneralResponseViewModel>(
                $"api/balance-general" +
                $"?clienteId={clienteId}" +
                $"&fechaHasta={fechaHasta:yyyy-MM-dd}");

            if (response.EsNoAutorizado)
                return RedirectToAction("Login", "Auth");
            if (!response.EsExitoso || response.Data is null)
            {
                TempData["Error"] = response.MensajeError ?? "No se pudo generar el balance general.";
                return View(vm);
            }

            vm.Resultado = response.Data;
        }

        return View(vm);
    }
}