using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/auxiliares")]
[Authorize]
public class AuxiliarController : ControllerBase
{
    private readonly IAuxiliarService _auxiliarService;

    public AuxiliarController(IAuxiliarService auxiliarService) => _auxiliarService = auxiliarService;

    /// <summary>El contador invita a un email como su auxiliar.</summary>
    [HttpPost("invitar")]
    public async Task<IActionResult> Invitar([FromBody] InvitarAuxiliarDto dto)
    {
        var contadorId = ObtenerUsuarioId();
        var resultado = await _auxiliarService.InvitarAuxiliar(contadorId, dto);
        return Ok(resultado);
    }

    /// <summary>Lista las invitaciones del contador autenticado.</summary>
    [HttpGet("invitaciones")]
    public async Task<IActionResult> ObtenerInvitaciones()
    {
        var contadorId = ObtenerUsuarioId();
        var resultado = await _auxiliarService.ObtenerInvitaciones(contadorId);
        return Ok(resultado);
    }

    /// <summary>El contador revoca el acceso de un auxiliar (queda Inactivo).</summary>
    [HttpDelete("{auxiliarId:guid}")]
    public async Task<IActionResult> Revocar(Guid auxiliarId)
    {
        var contadorId = ObtenerUsuarioId();
        await _auxiliarService.RevocarAuxiliar(contadorId, auxiliarId);
        return Ok(new { mensaje = "Auxiliar revocado exitosamente." });
    }

    private Guid ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario del token.");
        return id;
    }
}
