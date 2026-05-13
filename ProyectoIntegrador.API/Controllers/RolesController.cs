using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    /// <summary>Lista todos los roles con sus permisos.</summary>
    [HttpGet]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _rolService.ObtenerTodos());

    /// <summary>Obtiene un rol por Id.</summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
        => Ok(await _rolService.ObtenerPorId(id));

    /// <summary>Crea un rol custom (no predefinido). Arranca sin permisos.</summary>
    [HttpPost]
    [RequierePermiso("Usuarios", "Crear")]
    public async Task<IActionResult> Crear([FromBody] CrearRolDto dto)
    {
        var resultado = await _rolService.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Actualiza el nombre de un rol no predefinido.</summary>
    [HttpPut("{id:guid}")]
    [RequierePermiso("Usuarios", "Editar")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] CrearRolDto dto)
  => Ok(await _rolService.Actualizar(id, dto));

    /// <summary>Asigna un permiso a un rol.</summary>
    [HttpPost("{id:guid}/permisos")]
    [RequierePermiso("Usuarios", "Editar")]
    public async Task<IActionResult> AsignarPermiso(Guid id, [FromBody] AsignarPermisoDto dto)
    {
        await _rolService.AsignarPermiso(id, dto.PermisoId);
        return Ok(new { mensaje = "Permiso asignado correctamente." });
    }

    /// <summary>Remueve un permiso de un rol.</summary>
    [HttpDelete("{id:guid}/permisos/{permisoId:guid}")]
    [RequierePermiso("Usuarios", "Editar")]
    public async Task<IActionResult> RemoverPermiso(Guid id, Guid permisoId)
    {
        await _rolService.RemoverPermiso(id, permisoId);
        return Ok(new { mensaje = "Permiso removido correctamente." });
    }
}
