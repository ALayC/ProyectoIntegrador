using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly ApiClient _apiClient;

    public UsuariosController(ApiClient apiClient) => _apiClient = apiClient;

    // ── GET /Usuarios ─────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1, int cantidadPorPagina = 10,
        string? filtroRol = null, string? filtroEstado = null)
    {
        var response = await _apiClient.GetAsync<List<UsuarioApiDto>>("api/usuarios");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError;
            return View(new UsuarioIndexViewModel());
        }

        var todos = response.Data ?? [];

        // Filtros
        if (!string.IsNullOrEmpty(filtroRol))
            todos = todos.Where(u => u.Rol == filtroRol).ToList();
        if (!string.IsNullOrEmpty(filtroEstado))
            todos = todos.Where(u => u.Estado == filtroEstado).ToList();

        // Paginado en memoria
        var total = todos.Count;
        var items = todos
      .Skip((pagina - 1) * cantidadPorPagina)
     .Take(cantidadPorPagina)
    .Select(u => new UsuarioListItemViewModel
    {
        Id = u.Id,
        NombreCompleto = u.NombreCompleto,
        Email = u.Email,
        Rol = u.Rol,
        Estado = u.Estado
    }).ToList();

        var vm = new UsuarioIndexViewModel
        {
            Usuarios = items,
            Pagina = pagina,
            CantidadPorPagina = cantidadPorPagina,
            TotalRegistros = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)cantidadPorPagina),
            FiltroRol = filtroRol,
            FiltroEstado = filtroEstado
        };

        return View(vm);
    }

    // ── GET /Usuarios/Crear ───────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new CrearUsuarioViewModel();
        await PopularSelectsCrear(vm);
        return View(vm);
    }

    // ── POST /Usuarios/Crear ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopularSelectsCrear(model);
            return View(model);
        }

        var response = await _apiClient.PostAsync<object>("api/usuarios", new
        {
            email = model.Email,
            contrasena = model.Contrasena,
            nombreCompleto = model.NombreCompleto,
            rolId = model.RolId,
            contadorId = model.ContadorId
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al crear el usuario.");
            await PopularSelectsCrear(model);
            return View(model);
        }

        TempData["Exito"] = "Usuario creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Usuarios/Editar/{id} ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var response = await _apiClient.GetAsync<UsuarioApiDto>($"api/usuarios/{id}");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var u = response.Data;
        var vm = new EditarUsuarioViewModel
        {
            Id = u.Id,
            Email = u.Email,
            NombreCompleto = u.NombreCompleto,
            RolId = u.RolId,
            ContadorId = u.ContadorId,
            Estado = u.Estado
        };

        await PopularSelectsEditar(vm);
        return View(vm);
    }

    // ── POST /Usuarios/Editar/{id} ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopularSelectsEditar(model);
            return View(model);
        }

        var response = await _apiClient.PutAsync<object>($"api/usuarios/{id}", new
        {
            nombreCompleto = model.NombreCompleto,
            rolId = model.RolId,
            contadorId = model.ContadorId
        });

        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al actualizar el usuario.");
            await PopularSelectsEditar(model);
            return View(model);
        }

        TempData["Exito"] = "Usuario actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Usuarios/Desactivar/{id} ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        var response = await _apiClient.PatchAsync($"api/usuarios/{id}/desactivar");
        if (response.EsNoAutorizado) return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "Error al desactivar el usuario.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Exito"] = "Usuario desactivado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private async Task PopularSelectsCrear(CrearUsuarioViewModel vm)
    {
        vm.Roles = await ObtenerRolesSelect();
        vm.Contadores = await ObtenerContadoresSelect();
    }

    private async Task PopularSelectsEditar(EditarUsuarioViewModel vm)
    {
        vm.Roles = await ObtenerRolesSelect();
        vm.Contadores = await ObtenerContadoresSelect();
    }

    private async Task<List<RolSelectItem>> ObtenerRolesSelect()
    {
        var response = await _apiClient.GetAsync<List<RolApiDto>>("api/roles");
        if (!response.EsExitoso || response.Data is null) return [];
        return response.Data.Select(r => new RolSelectItem { Id = r.Id, Nombre = r.Nombre }).ToList();
    }

    private async Task<List<ContadorSelectItem>> ObtenerContadoresSelect()
    {
        var response = await _apiClient.GetAsync<List<UsuarioApiDto>>("api/usuarios");
        if (!response.EsExitoso || response.Data is null) return [];
        return response.Data
 .Where(u => u.Rol == "Contador" && u.Estado == "Activo")
 .Select(u => new ContadorSelectItem { Id = u.Id, NombreCompleto = u.NombreCompleto })
            .ToList();
    }

    // ── DTOs internos para deserializar la API ────────────────────────────────
    private class UsuarioApiDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public Guid RolId { get; set; }
        public Guid? ContadorId { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private class RolApiDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
