using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/cuentas-contables")]
[Authorize]
public class CuentasContablesController : ControllerBase
{
    private readonly ICuentaContableService _cuentaContableService;

    public CuentasContablesController(ICuentaContableService cuentaContableService)
    {
        _cuentaContableService = cuentaContableService;
    }

    /// <summary>Obtiene una cuenta contable por Id.</summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Cuentas", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
        => Ok(await _cuentaContableService.ObtenerPorId(id));

    /// <summary>Lista cuentas contables de un plan con paginación.</summary>
    [HttpGet]
    [RequierePermiso("Cuentas", "Consultar")]
    public async Task<IActionResult> ObtenerPorPlan([FromQuery] Guid planId, [FromQuery] int pagina = 1, [FromQuery] int cantidad = 20)
    {
        var resultado = await _cuentaContableService.ObtenerPorPlanDeCuentas(planId, pagina, cantidad);
        return Ok(resultado);
    }

    [HttpGet("arbol")]
    [RequierePermiso("Cuentas", "Consultar")]
    public async Task<IActionResult> ObtenerArbol([FromQuery] Guid planId)
    {
        var resultado = await _cuentaContableService.ObtenerArbol(planId);
        return Ok(resultado);
    }

    /// <summary>Crea una cuenta contable en el plan indicado.</summary>
    [HttpPost]
    [RequierePermiso("Cuentas", "Crear")]
    public async Task<IActionResult> Crear([FromQuery] Guid planId, [FromBody] CrearCuentaContableDto dto)
    {
        var resultado = await _cuentaContableService.Crear(planId, dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Actualiza una cuenta contable existente.</summary>
    [HttpPut("{id:guid}")]
    [RequierePermiso("Cuentas", "Editar")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarCuentaContableDto dto)
        => Ok(await _cuentaContableService.Actualizar(id, dto));

    /// <summary>Desactiva una cuenta contable (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [RequierePermiso("Cuentas", "Desactivar")]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        await _cuentaContableService.Desactivar(id);
        return Ok(new { mensaje = "Cuenta contable desactivada correctamente." });
    }
}
