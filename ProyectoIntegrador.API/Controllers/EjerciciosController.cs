using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/ejercicios")]
[Authorize]
public class EjerciciosController : ControllerBase
{
    private readonly IEjercicioContableService _ejercicioService;

    public EjerciciosController(IEjercicioContableService ejercicioService)
    {
        _ejercicioService = ejercicioService;
    }

    /// <summary>Lista ejercicios contables por cliente con paginación.</summary>
    [HttpGet]
    [RequierePermiso("Ejercicios", "Consultar")]
    public async Task<IActionResult> ObtenerPorCliente(
        [FromQuery] Guid clienteId,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidadPorPagina = 20)
    {
        var resultado = await _ejercicioService.ObtenerPorCliente(clienteId, pagina, cantidadPorPagina);
        return Ok(resultado);
    }

    /// <summary>Obtiene un ejercicio contable por Id.</summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Ejercicios", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
        => Ok(await _ejercicioService.ObtenerPorId(id));

    /// <summary>Crea un ejercicio contable.</summary>
    [HttpPost]
    [RequierePermiso("Ejercicios", "Crear")]
    public async Task<IActionResult> Crear([FromBody] CrearEjercicioContableDto dto)
    {
        var resultado = await _ejercicioService.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Actualiza un ejercicio contable existente.</summary>
    [HttpPut("{id:guid}")]
    [RequierePermiso("Ejercicios", "Editar")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarEjercicioContableDto dto)
        => Ok(await _ejercicioService.Actualizar(id, dto));

    /// <summary>Cierra un ejercicio contable.</summary>
    [HttpPost("{id:guid}/cerrar")]
    [RequierePermiso("Ejercicios", "Editar")]
    public async Task<IActionResult> Cerrar(Guid id)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        await _ejercicioService.Cerrar(id, usuarioId);
        return Ok(new { mensaje = "Ejercicio contable cerrado correctamente." });
    }

    private Guid ObtenerUsuarioIdDelToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario del token.");
        return id;
    }
}
