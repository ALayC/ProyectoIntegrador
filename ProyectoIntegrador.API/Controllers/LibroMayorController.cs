using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/libro-mayor")]
[Authorize]
public class LibroMayorController : ControllerBase
{
    private readonly ILibroMayorService _libroMayorService;

    public LibroMayorController(ILibroMayorService libroMayorService)
    {
        _libroMayorService = libroMayorService;
    }

    /// <summary>Consulta el libro mayor por cliente, cuentas y período.</summary>
    [HttpGet]
    [RequierePermiso("Reportes", "Consultar")]
    public async Task<ActionResult<LibroMayorResponseDto>> Obtener(
        [FromQuery] Guid clienteId,
        [FromQuery] List<Guid>? cuentaIds,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        [FromQuery] Guid? ejercicioId)
    {
        var filtro = new LibroMayorFiltroDto
        {
            ClienteId = clienteId,
            CuentaIds = cuentaIds,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            EjercicioId = ejercicioId
        };

        var resultado = await _libroMayorService.Obtener(filtro);
        return Ok(resultado);
    }
}
