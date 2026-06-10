using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService) => _auditoriaService = auditoriaService;

    /// <summary>
    /// Consulta registros de auditoría con filtros opcionales y paginación.
    /// Solo accesible para Administrador.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Consultar(
        [FromQuery] Guid? usuarioId,
        [FromQuery] string? entidad,
        [FromQuery] string? accion,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidadPorPagina = 20)
    {
        VerificarAdministrador();

        var resultado = await _auditoriaService.Consultar(
            usuarioId, entidad, accion, fechaDesde, fechaHasta, pagina, cantidadPorPagina);

        return Ok(resultado);
    }

    // ??????????????????????????????????????????????
    private void VerificarAdministrador()
    {
        var rol = User.FindFirst("rol")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
        if (rol != "Administrador")
            throw new AccesoNoAutorizadoException("Solo el Administrador puede consultar la auditoría.");
    }
}