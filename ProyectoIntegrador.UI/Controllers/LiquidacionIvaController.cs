using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class LiquidacionIvaController : Controller
{
    private readonly ApiClient _apiClient;

    public LiquidacionIvaController(ApiClient apiClient)
        => _apiClient = apiClient;

    [HttpGet]
    public async Task<IActionResult> Index(Guid clienteId, int? mes, int? anio)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!clienteResponse.EsExitoso || clienteResponse.Data is null)
        {
            TempData["Error"] = "No se pudo cargar el cliente.";
            return RedirectToAction("Index", "Clientes");
        }

        var vm = new LiquidacionIvaViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Mes = mes,
            Anio = anio
        };

        if (mes.HasValue && anio.HasValue)
        {
            var response = await _apiClient.GetAsync<LiquidacionIvaResultadoViewModel>(
                $"api/liquidacion-iva?clienteId={clienteId}&mes={mes}&anio={anio}");

            if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");
            if (!response.EsExitoso || response.Data is null)
            {
                TempData["Error"] = response.MensajeError ?? "No se pudo calcular la liquidación de IVA.";
                return View(vm);
            }

            vm.Resultado = response.Data;
        }

        return View(vm);
    }
}
