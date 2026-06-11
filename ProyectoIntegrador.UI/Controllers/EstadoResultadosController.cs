using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class EstadoResultadosController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IEstadoResultadosExcelService _excelService;
    private readonly IEstadoResultadosPdfService _pdfService;

    public EstadoResultadosController(
        ApiClient apiClient,
        IEstadoResultadosExcelService excelService,
        IEstadoResultadosPdfService pdfService)
    {
        _apiClient = apiClient;
        _excelService = excelService;
        _pdfService = pdfService;
    }

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
                $"api/estado-resultados?clienteId={clienteId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");

            if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");
            if (!response.EsExitoso || response.Data is null)
            {
                TempData["Error"] = response.MensajeError ?? "No se pudo generar el estado de resultados.";
                return View(vm);
            }

            vm.Resultado = response.Data;
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarExcel(Guid clienteId, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var (vm, error) = await ConstruirViewModel(clienteId, fechaDesde, fechaHasta);
        if (error != null) return error;

        var bytes = _excelService.Generar(vm!);
        var nombre = $"EstadoResultados_{vm!.ClienteNombre}_{DateTime.Today:yyyyMMdd}.xlsx".Replace(" ", "_");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(Guid clienteId, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var (vm, error) = await ConstruirViewModel(clienteId, fechaDesde, fechaHasta);
        if (error != null) return error;

        var bytes = _pdfService.Generar(vm!);
        var nombre = $"EstadoResultados_{vm!.ClienteNombre}_{DateTime.Today:yyyyMMdd}.pdf".Replace(" ", "_");
        return File(bytes, "application/pdf", nombre);
    }

    private async Task<(EstadoResultadosViewModel? vm, IActionResult? error)> ConstruirViewModel(
        Guid clienteId, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado)
            return (null, RedirectToAction("Login", "Auth"));

        var response = await _apiClient.GetAsync<EstadoResultadosResponseViewModel>(
            $"api/estado-resultados?clienteId={clienteId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");

        if (response.EsNoAutorizado)
            return (null, RedirectToAction("Login", "Auth"));

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo generar el estado de resultados.";
            return (null, RedirectToAction("Index", new { clienteId, fechaDesde, fechaHasta }));
        }

        var vm = new EstadoResultadosViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Resultado = response.Data
        };

        return (vm, null);
    }
}