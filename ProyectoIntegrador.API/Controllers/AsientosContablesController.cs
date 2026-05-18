using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/asientos-contables")]
[Authorize]
public class AsientosContablesController : ControllerBase
{
    private readonly IAsientoContableService _asientoService;

    public AsientosContablesController(IAsientoContableService asientoService)
    {
        _asientoService = asientoService;
    }

    /// <summary>Obtiene un asiento contable completo con sus líneas.</summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Asientos", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
        => Ok(await _asientoService.ObtenerPorId(id));

    /// <summary>Lista asientos del Libro Diario con filtros y paginación.</summary>
    [HttpGet]
    [RequierePermiso("Asientos", "Consultar")]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid clienteId,
        [FromQuery] Guid? ejercicioId,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidadPorPagina = 20)
    {
        var filtro = new FiltroAsientoDto
        {
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = pagina,
            CantidadPorPagina = cantidadPorPagina
        };

        var (items, total) = await _asientoService.Listar(filtro);

        return Ok(new
        {
            items,
            total,
            pagina,
            cantidadPorPagina,
            totalPaginas = (int)Math.Ceiling((double)total / cantidadPorPagina)
        });
    }

    /// <summary>Registra un nuevo asiento contable.</summary>
    [HttpPost]
    [RequierePermiso("Asientos", "Crear")]
    public async Task<IActionResult> Crear([FromBody] CrearAsientoContableDto dto)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _asientoService.Crear(dto, usuarioId);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Revierte un asiento contable generando un asiento inverso.</summary>
    [HttpPost("{id:guid}/revertir")]
    [RequierePermiso("Asientos", "Revertir")]
    public async Task<IActionResult> Revertir(Guid id)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _asientoService.Revertir(id, usuarioId);
        return Ok(resultado);
    }

    // ──────────────────────────────────────────────
    private Guid ObtenerUsuarioIdDelToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new AccesoNoAutorizadoException("No se pudo obtener el ID del usuario del token.");
        return id;
    }
}