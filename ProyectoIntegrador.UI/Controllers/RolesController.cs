using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize(Roles = "Administrador")]
public class RolesController : Controller
{
    private readonly ApiClient _apiClient;

    public RolesController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // ── GET /Roles ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var rolesResp = await _apiClient.GetAsync<List<RolApiDto>>("api/roles");
        var permisosResp = await _apiClient.GetAsync<List<PermisoApiDto>>("api/permisos");

        if (rolesResp.EsNoAutorizado || permisosResp.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!rolesResp.EsExitoso)
        {
            TempData["Error"] = rolesResp.MensajeError;
            return View(new RolIndexViewModel());
        }

        var todosPermisos = permisosResp.Data ?? [];
        var roles = rolesResp.Data ?? [];

        var vm = new RolIndexViewModel
        {
            Roles = roles.Select(r => new RolItemViewModel
            {
                Id = r.Id,
                Nombre = r.Nombre,
                EsPredefinido = r.EsPredefinido,
                Permisos = r.Permisos.Select(p => new PermisoItemViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Modulo = p.Modulo,
                    Accion = p.Accion,
                    Asignado = true
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }

    // ── GET /Roles/Gestionar/{id} ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Gestionar(Guid id)
    {
        var rolResp = await _apiClient.GetAsync<RolApiDto>($"api/roles/{id}");
        var todosPermsResp = await _apiClient.GetAsync<List<PermisoApiDto>>("api/permisos");

        if (rolResp.EsNoAutorizado || todosPermsResp.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!rolResp.EsExitoso || rolResp.Data is null)
        {
            TempData["Error"] = rolResp.MensajeError ?? "Rol no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var rol = rolResp.Data;
        var todosPermisos = todosPermsResp.Data ?? [];
        var permisosDelRol = rol.Permisos.Select(p => p.Id).ToHashSet();

        var modulos = todosPermisos
              .GroupBy(p => p.Modulo)
             .Select(g => new ModuloPermisosViewModel
             {
                 Nombre = g.Key,
                 Permisos = g.Select(p => new PermisoCheckViewModel
                 {
                     Id = p.Id,
                     Nombre = p.Nombre,
                     Accion = p.Accion,
                     Asignado = permisosDelRol.Contains(p.Id)
                 }).ToList()
             }).ToList();

        var vm = new GestionPermisosViewModel
        {
            RolId = rol.Id,
            NombreRol = rol.Nombre,
            EsPredefinido = rol.EsPredefinido,
            Modulos = modulos
        };

        return View(vm);
    }

    // ── POST /Roles/AsignarPermiso ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarPermiso(Guid rolId, Guid permisoId)
    {
        var response = await _apiClient.PostAsync<object>($"api/roles/{rolId}/permisos", new { permisoId });
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
            TempData["Error"] = response.MensajeError ?? "Error al asignar permiso.";

        return RedirectToAction(nameof(Gestionar), new { id = rolId });
    }

    // ── POST /Roles/RemoverPermiso ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverPermiso(Guid rolId, Guid permisoId)
    {
        var response = await _apiClient.DeleteAsync($"api/roles/{rolId}/permisos/{permisoId}");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
            TempData["Error"] = response.MensajeError ?? "Error al remover permiso.";

        return RedirectToAction(nameof(Gestionar), new { id = rolId });
    }

    // ── GET /Roles/Crear ──────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Crear() => View(new CrearRolViewModel());

    // ── POST /Roles/Crear ─────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearRolViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _apiClient.PostAsync<object>("api/roles", new { nombre = model.Nombre });
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al crear el rol.");
            return View(model);
        }

        TempData["Exito"] = "Rol creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Roles/EditarNombre ──────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarNombre(Guid id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        var response = await _apiClient.PutAsync<object>($"api/roles/{id}", new { nombre });
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
            TempData["Error"] = response.MensajeError ?? "Error al actualizar el rol.";
        else
            TempData["Exito"] = "Nombre del rol actualizado.";

        return RedirectToAction(nameof(Index));
    }

    // ── DTOs internos ─────────────────────────────────────────────────────────
    private class RolApiDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsPredefinido { get; set; }
        public List<PermisoApiDto> Permisos { get; set; } = [];
    }

    private class PermisoApiDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
    }
}
