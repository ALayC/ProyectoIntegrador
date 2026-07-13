using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize(Roles = "Contador,Administrador")]
public class AuxiliaresController : Controller
{
    private readonly ApiClient _apiClient;

    public AuxiliaresController(ApiClient apiClient) => _apiClient = apiClient;

    // GET /Auxiliares
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new AuxiliarViewModel();

        // Invitaciones
        var respInv = await _apiClient.GetAsync<List<InvitacionAuxiliarApiDto>>("api/auxiliares/invitaciones");
        if (respInv.EsNoAutorizado) return RedirectToAction("Login", "Auth");
        if (respInv.EsExitoso && respInv.Data is not null)
        {
            vm.Invitaciones = respInv.Data.Select(i => new InvitacionAuxiliarViewModel
            {
                Id = i.Id,
                Email = i.Email,
                Estado = i.Estado,
                FechaCreacion = i.FechaCreacion,
                FechaExpiracion = i.FechaExpiracion
            }).ToList();
        }

        // Auxiliares activos (endpoint dedicado)
        var respAux = await _apiClient.GetAsync<List<AuxiliarActivoApiDto>>("api/usuarios?filtroRolNombre=Auxiliar Contable");
        if (respAux.EsExitoso && respAux.Data is not null)
        {
            vm.Auxiliares = respAux.Data
                .Select(u => new AuxiliarActivoViewModel
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Estado = u.Estado
                }).ToList();
        }

        return View(vm);
    }

    // POST /Auxiliares/Invitar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invitar(AuxiliarViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "El email ingresado no es válido.";
            return RedirectToAction(nameof(Index));
        }

        var resp = await _apiClient.PostAsync<object>(
            "api/auxiliares/invitar",
            new { email = model.FormInvitar.Email });

        if (resp.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (resp.EsExitoso)
            TempData["Success"] = $"Invitación enviada a {model.FormInvitar.Email}.";
        else
            TempData["Error"] = resp.MensajeError ?? "No se pudo enviar la invitación.";

        return RedirectToAction(nameof(Index));
    }

    // POST /Auxiliares/Revocar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revocar(Guid id)
    {
        var resp = await _apiClient.DeleteAsync($"api/auxiliares/{id}");

        if (resp.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (resp.EsExitoso)
            TempData["Success"] = "Auxiliar revocado correctamente.";
        else
            TempData["Error"] = resp.MensajeError ?? "No se pudo revocar el auxiliar.";

        return RedirectToAction(nameof(Index));
    }
}
