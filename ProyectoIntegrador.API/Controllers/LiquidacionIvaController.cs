using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/liquidacion-iva")]
[Authorize]
public class LiquidacionIvaController : ControllerBase
{
    private readonly ILiquidacionIvaService _liquidacionIvaService;

    public LiquidacionIvaController(ILiquidacionIvaService liquidacionIvaService)
        => _liquidacionIvaService = liquidacionIvaService;

    /// <summary>Calcula la liquidación de IVA de un mes/año dado para un cliente.</summary>
    [HttpGet]
    [RequierePermiso("Reportes", "Consultar")]
    public async Task<IActionResult> Calcular(
        [FromQuery] Guid clienteId,
        [FromQuery] int mes,
        [FromQuery] int anio)
    {
        var resultado = await _liquidacionIvaService.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes = mes,
            Anio = anio
        });

        return Ok(resultado);
    }
}
