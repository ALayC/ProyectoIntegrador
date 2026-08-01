using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class BalanceGeneralController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IBalanceGeneralExcelService _excelService;
    private readonly IBalanceGeneralPdfService _pdfService;

    public BalanceGeneralController(
        ApiClient apiClient,
        IBalanceGeneralExcelService excelService,
        IBalanceGeneralPdfService pdfService)
    {
        _apiClient = apiClient;
        _excelService = excelService;
        _pdfService = pdfService;
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

    [HttpGet]
    public async Task<IActionResult> ExportarExcel(Guid clienteId, DateOnly? fechaHasta)
    {
        var (vm, error) = await ConstruirViewModel(clienteId, fechaHasta);
        if (error != null) return error;

        var bytes = _excelService.Generar(vm!);
        var nombre = $"BalanceGeneral_{vm!.ClienteNombre}_{DateTime.Today:yyyyMMdd}.xlsx".Replace(" ", "_");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(Guid clienteId, DateOnly? fechaHasta)
    {
        var (vm, error) = await ConstruirViewModel(clienteId, fechaHasta);
        if (error != null) return error;

        var bytes = _pdfService.Generar(vm!);
        var nombre = $"BalanceGeneral_{vm!.ClienteNombre}_{DateTime.Today:yyyyMMdd}.pdf".Replace(" ", "_");
        return File(bytes, "application/pdf", nombre);
    }

    private async Task<(BalanceGeneralViewModel? vm, IActionResult? error)> ConstruirViewModel(
        Guid clienteId, DateOnly? fechaHasta)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado)
            return (null, RedirectToAction("Login", "Auth"));

        var response = await _apiClient.GetAsync<BalanceGeneralResponseViewModel>(
            $"api/balance-general" +
            $"?clienteId={clienteId}" +
            $"&fechaHasta={fechaHasta:yyyy-MM-dd}");

        if (response.EsNoAutorizado)
            return (null, RedirectToAction("Login", "Auth"));

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo generar el balance general.";
            return (null, RedirectToAction("Index", new { clienteId, fechaHasta }));
        }

        var vm = new BalanceGeneralViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty,
            FechaHasta = fechaHasta,
            Resultado = response.Data
        };

        return (vm, null);
    }
}
