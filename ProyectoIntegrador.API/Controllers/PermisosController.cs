using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/permisos")]
[Authorize]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _permisoService;

    public PermisosController(IPermisoService permisoService)
    {
    _permisoService = permisoService;
    }

    /// <summary>Lista todos los permisos del sistema.</summary>
    [HttpGet]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _permisoService.ObtenerTodos());

    /// <summary>Lista permisos filtrados por módulo.</summary>
    [HttpGet("modulo/{modulo}")]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerPorModulo(string modulo)
        => Ok(await _permisoService.ObtenerPorModulo(modulo));

    /// <summary>Lista permisos asignados a un rol.</summary>
  [HttpGet("rol/{rolId:guid}")]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerPorRol(Guid rolId)
  => Ok(await _permisoService.ObtenerPorRol(rolId));
}
