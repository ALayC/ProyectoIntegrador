using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/comprobantes")]
[Authorize]
public class ComprobantesController : ControllerBase
{
    private readonly IComprobanteService _comprobanteService;

    public ComprobantesController(IComprobanteService comprobanteService) => _comprobanteService = comprobanteService;

    [HttpPost]
    [RequierePermiso("Comprobantes", "Crear")]
    public async Task<IActionResult> Crear([FromBody] ComprobanteCrearDto dto)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _comprobanteService.Crear(dto, usuarioId);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id:guid}")]
    [RequierePermiso("Comprobantes", "Consultar")]
    public async Task<IActionResult> Obtener(Guid id)
        => Ok(await _comprobanteService.Obtener(id));

    [HttpGet]
    [RequierePermiso("Comprobantes", "Consultar")]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid clienteId,
        [FromQuery] string? tipo,
        [FromQuery] string? rut,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidadPorPagina = 20)
    {
        var filtro = new FiltroComprobanteDto
        {
            ClienteId = clienteId,
            Tipo = tipo,
            RUT = rut,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Estado = estado,
            Pagina = pagina,
            CantidadPorPagina = cantidadPorPagina
        };

        var items = await _comprobanteService.Listar(filtro);

        return Ok(new
        {
            items,
            pagina,
            cantidadPorPagina,
            total = items.Count
        });
    }

    [HttpPut("{id:guid}")]
    [RequierePermiso("Comprobantes", "Editar")]
    public async Task<IActionResult> Modificar(Guid id, [FromBody] ComprobanteModificarDto dto)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _comprobanteService.Modificar(id, dto, usuarioId);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/anular")]
    [RequierePermiso("Comprobantes", "Anular")]
    public async Task<IActionResult> Anular(Guid id)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        await _comprobanteService.Anular(id, usuarioId);
        return Ok(new { mensaje = "Comprobante anulado exitosamente." });
    }

    [HttpPost("{id:guid}/generar-asiento")]
    [RequierePermiso("Asientos", "Crear")]
    public async Task<IActionResult> GenerarAsiento(Guid id, [FromBody] GenerarAsientoDesdeComprobanteDto dto)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _comprobanteService.GenerarAsiento(id, dto, usuarioId);
        return Ok(resultado);
    }

    [HttpGet("/api/asientos/{asientoId:guid}/comprobante")]
    [RequierePermiso("Comprobantes", "Consultar")]
    public async Task<IActionResult> ObtenerPorAsiento(Guid asientoId)
        => Ok(await _comprobanteService.ObtenerPorAsiento(asientoId));

    private Guid ObtenerUsuarioIdDelToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new AccesoNoAutorizadoException("No se pudo obtener el ID del usuario del token.");
        return id;
    }
}
