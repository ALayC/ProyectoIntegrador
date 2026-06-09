using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    /// <summary>Lista todos los usuarios del sistema.</summary>
    [HttpGet]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _usuarioService.ObtenerTodos());

    /// <summary>Obtiene un usuario por Id.</summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Usuarios", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
        => Ok(await _usuarioService.ObtenerPorId(id));

    /// <summary>Crea un nuevo usuario (Admin, Contador o Auxiliar).</summary>
    [HttpPost]
    [RequierePermiso("Usuarios", "Crear")]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
    {
        var adminId = ObtenerUsuarioIdDelToken();
        var resultado = await _usuarioService.Crear(dto, adminId);
        return Created(string.Empty, resultado);
    }

    /// <summary>Edita nombre completo y/o ContadorId de un usuario.</summary>
    [HttpPut("{id:guid}")]
    [RequierePermiso("Usuarios", "Editar")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarUsuarioDto dto)
    {
        var adminId = ObtenerUsuarioIdDelToken();
        return Ok(await _usuarioService.Editar(id, dto, adminId));
    }

    /// <summary>Desactiva un usuario (soft delete).</summary>
    [HttpPatch("{id:guid}/desactivar")]
    [RequierePermiso("Usuarios", "Desactivar")]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        var adminId = ObtenerUsuarioIdDelToken();
        await _usuarioService.Desactivar(id, adminId);
        return Ok(new { mensaje = "Usuario desactivado correctamente." });
    }

    // ??????????????????????????????????????????????
    private Guid ObtenerUsuarioIdDelToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new AccesoNoAutorizadoException("No se pudo obtener el ID del usuario del token.");
        return id;
    }
}
