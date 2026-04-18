using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly ApiClient _apiClient;

    public ClientesController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // ?? GET /Clientes ?????????????????????????????
    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1, int cantidadPorPagina = 10)
    {
        var response = await _apiClient.GetAsync<PaginadoViewModel<ClienteListViewModel>>(
                $"api/clientes?pagina={pagina}&cantidadPorPagina={cantidadPorPagina}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError;
            return View(new PaginadoViewModel<ClienteListViewModel>());
        }

        return View(response.Data ?? new PaginadoViewModel<ClienteListViewModel>());
    }

    // ?? GET /Clientes/Crear ???????????????????????
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new ClienteViewModel());
    }

    // ?? POST /Clientes/Crear ??????????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiClient.PostAsync<ClienteListViewModel>("api/clientes", new
        {
            rut = model.Rut,
            razonSocial = model.RazonSocial,
            nombreFantasia = model.NombreFantasia,
            email = model.Email,
            telefono = model.Telefono,
            tipoContribuyente = model.TipoContribuyente,
            monedaBase = model.MonedaBase
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al crear el cliente.");
            return View(model);
        }

        TempData["Exito"] = "Cliente creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ?? GET /Clientes/Editar/{id} ?????????????????
    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var response = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{id}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Cliente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var cliente = response.Data;
        var viewModel = new ClienteViewModel
        {
            Rut = cliente.Rut,
            RazonSocial = cliente.RazonSocial,
            NombreFantasia = cliente.NombreFantasia,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            TipoContribuyente = cliente.TipoContribuyente,
            MonedaBase = cliente.MonedaBase
        };

        ViewData["ClienteId"] = id;
        ViewData["RutOriginal"] = cliente.Rut;
        return View(viewModel);
    }

    // ?? POST /Clientes/Editar/{id} ????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, ClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ClienteId"] = id;
            ViewData["RutOriginal"] = model.Rut;
            return View(model);
        }

        var response = await _apiClient.PutAsync<ClienteListViewModel>($"api/clientes/{id}", new
        {
            rut = model.Rut,
            razonSocial = model.RazonSocial,
            nombreFantasia = model.NombreFantasia,
            email = model.Email,
            telefono = model.Telefono,
            tipoContribuyente = model.TipoContribuyente,
            monedaBase = model.MonedaBase
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al actualizar el cliente.");
            ViewData["ClienteId"] = id;
            ViewData["RutOriginal"] = model.Rut;
            return View(model);
        }

        TempData["Exito"] = "Cliente actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ?? POST /Clientes/Desactivar/{id} ????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        var response = await _apiClient.PatchAsync($"api/clientes/{id}/desactivar");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "Error al desactivar el cliente.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Exito"] = "Cliente desactivado exitosamente.";
        return RedirectToAction(nameof(Index));
    }
}
