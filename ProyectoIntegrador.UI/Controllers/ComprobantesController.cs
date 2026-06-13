using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class ComprobantesController : Controller
{
    private readonly ApiClient _apiClient;

    public ComprobantesController(ApiClient apiClient) => _apiClient = apiClient;

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid clienteId,
        string? tipo,
        string? rut,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? estado,
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

        var query = new Dictionary<string, string?>
        {
            ["clienteId"] = clienteId.ToString(),
            ["tipo"] = tipo,
            ["rut"] = rut,
            ["fechaDesde"] = fechaDesde?.ToString("yyyy-MM-dd"),
            ["fechaHasta"] = fechaHasta?.ToString("yyyy-MM-dd"),
            ["estado"] = estado,
            ["pagina"] = pagina.ToString(),
            ["cantidadPorPagina"] = cantidadPorPagina.ToString()
        };

        var url = QueryHelpers.AddQueryString("api/comprobantes", query!);
        var response = await _apiClient.GetAsync<ComprobantesPaginadoResponse>(url);

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudieron cargar los comprobantes.";
            return RedirectToAction("Detalles", "Clientes", new { id = clienteId });
        }

        var vm = new ComprobanteIndexViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial,
            Tipo = tipo,
            RUT = rut,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Estado = estado,
            Pagina = pagina,
            CantidadPorPagina = cantidadPorPagina,
            Items = response.Data.Items,
            Total = response.Data.Total
        };

        return View(vm);
    }

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

        var vm = new ComprobanteCrearViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data.RazonSocial
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Guid clienteId, ComprobanteCrearViewModel model)
    {
        model.ClienteId = clienteId;

        if (!ModelState.IsValid)
        {
            await CargarCliente(model);
            return View(model);
        }

        var response = await _apiClient.PostAsync<ComprobanteDetalleViewModel>("api/comprobantes", new
        {
            model.ClienteId,
            model.Tipo,
            model.Numero,
            model.RUT,
            model.Fecha,
            model.ImporteNeto,
            model.TasaIVA,
            model.ImporteIVA,
            model.ImporteTotal
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo crear el comprobante.");
            await CargarCliente(model);
            return View(model);
        }

        TempData["Exito"] = "Comprobante creado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id = response.Data.Id, clienteId = model.ClienteId });
    }

    [HttpGet]
    public async Task<IActionResult> Detalles(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<ComprobanteDetalleViewModel>($"api/comprobantes/{id}");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo cargar el comprobante.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var ejerciciosResponse = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina=1&cantidadPorPagina=100");

        var cuentasResponse = await _apiClient.GetAsync<List<CuentaContableViewModel>>(
            $"api/clientes/{clienteId}/cuentas-imputables");

        ViewBag.ClienteId = clienteId;
        ViewBag.ClienteNombre = clienteResponse.Data?.RazonSocial ?? string.Empty;
        ViewBag.Ejercicios = ejerciciosResponse.Data?.Datos ?? new List<EjercicioContableViewModel>();
        ViewBag.CuentasImputables = cuentasResponse.Data ?? new List<CuentaContableViewModel>();

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarAsiento(Guid id, Guid clienteId, GenerarAsientoDesdeComprobanteViewModel model)
    {
        model.ComprobanteId = id;
        model.ClienteId = clienteId;

        var response = await _apiClient.PostAsync<AsientoContableViewModel>($"api/comprobantes/{id}/generar-asiento", new
        {
            model.EjercicioId,
            model.CuentaDebeId,
            model.CuentaHaberId,
            model.Fecha,
            model.Glosa
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo generar el asiento desde el comprobante.";
            return RedirectToAction(nameof(Detalles), new { id, clienteId });
        }

        TempData["Exito"] = $"Asiento N° {response.Data.Numero} generado correctamente en moneda nacional (UYU).";
        return RedirectToAction(nameof(Detalles), new { id, clienteId });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<ComprobanteDetalleViewModel>($"api/comprobantes/{id}");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo cargar el comprobante.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        if (response.Data.TieneAsiento)
        {
            TempData["Error"] = "No se puede editar un comprobante asociado a un asiento.";
            return RedirectToAction(nameof(Detalles), new { id, clienteId });
        }

        var vm = new ComprobanteEditarViewModel
        {
            Id = response.Data.Id,
            ClienteId = response.Data.ClienteId,
            Tipo = response.Data.Tipo,
            Numero = response.Data.Numero,
            RUT = response.Data.RUT,
            Fecha = response.Data.Fecha,
            ImporteNeto = response.Data.ImporteNeto,
            TasaIVA = response.Data.TasaIVA,
            ImporteIVA = response.Data.ImporteIVA,
            ImporteTotal = response.Data.ImporteTotal
        };

        await CargarCliente(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, Guid clienteId, ComprobanteEditarViewModel model)
    {
        model.Id = id;
        model.ClienteId = clienteId;

        if (!ModelState.IsValid)
        {
            await CargarCliente(model);
            return View(model);
        }

        var response = await _apiClient.PutAsync<ComprobanteDetalleViewModel>($"api/comprobantes/{id}", new
        {
            model.Tipo,
            model.Numero,
            model.RUT,
            model.Fecha,
            model.ImporteNeto,
            model.TasaIVA,
            model.ImporteIVA,
            model.ImporteTotal
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo editar el comprobante.");
            await CargarCliente(model);
            return View(model);
        }

        TempData["Exito"] = "Comprobante editado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id, clienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(Guid id, Guid clienteId)
    {
        var response = await _apiClient.PostAsync<object>($"api/comprobantes/{id}/anular", new { });
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo anular el comprobante.";
            return RedirectToAction(nameof(Detalles), new { id, clienteId });
        }

        TempData["Exito"] = "Comprobante anulado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id, clienteId });
    }

    private async Task CargarCliente(ComprobanteCrearViewModel model)
    {
        var cliente = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{model.ClienteId}");
        model.ClienteNombre = cliente.Data?.RazonSocial ?? string.Empty;
    }

    private async Task CargarCliente(ComprobanteEditarViewModel model)
    {
        var cliente = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{model.ClienteId}");
        model.ClienteNombre = cliente.Data?.RazonSocial ?? string.Empty;
    }

    private class ComprobantesPaginadoResponse
    {
        public List<ComprobanteResumenViewModel> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int CantidadPorPagina { get; set; }
    }
}
