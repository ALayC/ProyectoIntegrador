using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/tipocambio")]
[Authorize]
public class TipoDeCambioController : ControllerBase
{
    private readonly ITipoDeCambioService _tipoDeCambioService;

    public TipoDeCambioController(ITipoDeCambioService tipoDeCambioService)
        => _tipoDeCambioService = tipoDeCambioService;

    /// <summary>
    /// Devuelve el tipo de cambio venta para una moneda y fecha dadas.
    /// Si no existe en BD lo consulta en BCU; si BCU no responde usa el último disponible.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCotizacion(
        [FromQuery] string moneda,
        [FromQuery] DateOnly fecha)
    {
        if (string.IsNullOrWhiteSpace(moneda))
            return BadRequest("El parámetro 'moneda' es obligatorio.");

        if (moneda.ToUpperInvariant() == "UYU")
            return Ok(new { moneda = "UYU", fecha, valor = 1m, fuente = "Local" });

        var resultado = await _tipoDeCambioService.ObtenerCotizacionDetalle(moneda, fecha);

        return Ok(new
        {
            moneda = moneda.ToUpperInvariant(),
            fecha = resultado.FechaReal,
            valor = resultado.Valor,
            fuente = "BCU"
        });
    }

    /// <summary>
    /// Devuelve el último tipo de cambio venta disponible para una moneda.
    /// Útil para pre-completar el campo T/C en formularios.
    /// </summary>
    [HttpGet("ultimo")]
    public async Task<IActionResult> ObtenerUltimo([FromQuery] string moneda)
    {
        if (string.IsNullOrWhiteSpace(moneda))
            return BadRequest("El parámetro 'moneda' es obligatorio.");

        if (moneda.ToUpperInvariant() == "UYU")
            return Ok(new { moneda = "UYU", valor = 1m, fuente = "Local" });

        var valor = await _tipoDeCambioService.ObtenerUltimoTipoCambioVenta(moneda);

        return Ok(new
        {
            moneda = moneda.ToUpperInvariant(),
            valor,
            fuente = "BCU"
        });
    }

    /// <summary>
    /// Sincroniza tipos de cambio desde BCU para un rango de fechas.
    /// </summary>
    [HttpPost("sincronizar")]
    public async Task<IActionResult> Sincronizar(
        [FromQuery] string moneda,
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta)
    {
        if (string.IsNullOrWhiteSpace(moneda))
            return BadRequest("El parámetro 'moneda' es obligatorio.");

        await _tipoDeCambioService.SincronizarDesdeBCU(moneda, fechaDesde, fechaHasta);
        return Ok(new { mensaje = $"Sincronización completada para {moneda.ToUpperInvariant()} ({fechaDesde:dd/MM/yyyy} – {fechaHasta:dd/MM/yyyy})." });
    }
}
