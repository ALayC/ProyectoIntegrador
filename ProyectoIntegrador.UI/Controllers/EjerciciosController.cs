using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class EjerciciosController : Controller
{
    private readonly ApiClient _apiClient;

    public EjerciciosController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid clienteId, int pagina = 1, int cantidadPorPagina = 10)
    {
        var response = await _apiClient.GetAsync<PaginadoViewModel<EjercicioContableViewModel>>(
            $"api/ejercicios?clienteId={clienteId}&pagina={pagina}&cantidadPorPagina={cantidadPorPagina}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        var clienteResponse = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
        if (clienteResponse.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudieron cargar los ejercicios contables.";
            return View(new EjercicioContableIndexViewModel
            {
                ClienteId = clienteId,
                ClienteNombre = clienteResponse.Data?.RazonSocial,
                Paginado = new PaginadoViewModel<EjercicioContableViewModel>()
            });
        }

        var paginado = response.Data ?? new PaginadoViewModel<EjercicioContableViewModel>();
        var tieneAbierto = paginado.Datos.Any(e => e.Estado == "Abierto");

        return View(new EjercicioContableIndexViewModel
        {
            ClienteId = clienteId,
            ClienteNombre = clienteResponse.Data?.RazonSocial,
            Paginado = paginado,
            TieneEjercicioAbierto = tieneAbierto
        });
    }

    [HttpGet]
    public IActionResult Crear(Guid clienteId)
    {
        return View(new EjercicioContableFormViewModel { ClienteId = clienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EjercicioContableFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiClient.PostAsync<EjercicioContableViewModel>("api/ejercicios", new
        {
            clienteId = model.ClienteId,
            fechaInicio = model.FechaInicio,
            fechaFin = model.FechaFin
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo crear el ejercicio contable.");
            return View(model);
        }

        TempData["Exito"] = "Ejercicio contable creado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<EjercicioContableViewModel>($"api/ejercicios/{id}");

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Ejercicio contable no encontrado.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        return View(new EjercicioContableFormViewModel
        {
            Id = id,
            ClienteId = clienteId,
            FechaInicio = response.Data.FechaInicio,
            FechaFin = response.Data.FechaFin
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EjercicioContableFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var response = await _apiClient.PutAsync<EjercicioContableViewModel>($"api/ejercicios/{id}", new
        {
            fechaInicio = model.FechaInicio,
            fechaFin = model.FechaFin
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "No se pudo actualizar el ejercicio contable.");
            return View(model);
        }

        TempData["Exito"] = "Ejercicio contable actualizado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(Guid id, Guid clienteId)
    {
        var response = await _apiClient.PostAsync<object>($"api/ejercicios/{id}/cerrar", new { });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "No se pudo cerrar el ejercicio contable.";
            return RedirectToAction(nameof(Index), new { clienteId });
        }

        TempData["Exito"] = "Ejercicio contable cerrado correctamente.";
        return RedirectToAction(nameof(Index), new { clienteId });
    }
}
