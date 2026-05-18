using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Obtiene los clientes del contador autenticado con paginación.
    /// </summary>
    [HttpGet]
    [RequierePermiso("Clientes", "Consultar")]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] int pagina = 1,
     [FromQuery] int cantidadPorPagina = 20)
    {
        var contadorId = ObtenerUsuarioIdDelToken();
        var resultado = await _clienteService.ObtenerPorContador(contadorId, pagina, cantidadPorPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene un cliente por su Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequierePermiso("Clientes", "Consultar")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var resultado = await _clienteService.ObtenerPorId(id);
        return Ok(resultado);
    }

    /// <summary>
    /// Crea un nuevo cliente. El contadorId se toma del JWT.
    /// </summary>
    [HttpPost]
    [RequierePermiso("Clientes", "Crear")]
    public async Task<IActionResult> Crear([FromBody] ClienteDto clienteDto)
    {
        var contadorId = ObtenerUsuarioIdDelToken();
        var resultado = await _clienteService.Crear(clienteDto, contadorId);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>
    /// Actualiza los datos de un cliente existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequierePermiso("Clientes", "Editar")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ClienteDto clienteDto)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _clienteService.Actualizar(id, clienteDto, usuarioId);
        return Ok(resultado);
    }

    /// <summary>
    /// Desactiva un cliente (soft delete).
    /// </summary>
    [HttpPatch("{id:guid}/desactivar")]
    [RequierePermiso("Clientes", "Desactivar")]
    public async Task<IActionResult> Desactivar(Guid id)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        await _clienteService.Desactivar(id, usuarioId);
        return Ok(new { mensaje = "Cliente desactivado exitosamente." });
    }

    /// <summary>
    /// Activa un cliente.
    /// </summary>
    [HttpPatch("{id:guid}/activar")]
    [RequierePermiso("Clientes", "Desactivar")]
    public async Task<IActionResult> Activar(Guid id)
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        await _clienteService.Activar(id, usuarioId);
        return Ok(new { mensaje = "Cliente activado exitosamente." });
    }

    /// <summary>
    /// Obtiene el plan de cuentas del cliente en formato árbol.
    /// </summary>
    [HttpGet("{id:guid}/plan-de-cuentas")]
    [RequierePermiso("Clientes", "Consultar")]
    public async Task<IActionResult> ObtenerPlanDeCuentas(Guid id)
    {
        var resultado = await _clienteService.ObtenerPlanDeCuentas(id);
        return Ok(resultado);
    }

    /// <summary>Obtiene solo las cuentas imputables del cliente (para selects de asientos).</summary>
    [HttpGet("{id:guid}/cuentas-imputables")]
    [RequierePermiso("Clientes", "Consultar")]
    public async Task<IActionResult> ObtenerCuentasImputables(Guid id)
        => Ok(await _clienteService.ObtenerCuentasImputables(id));

    // ??????????????????????????????????????????????
    // Helper privado
    // ??????????????????????????????????????????????
    private Guid ObtenerUsuarioIdDelToken()
    {
        var claimSub = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (claimSub is null || !Guid.TryParse(claimSub.Value, out var usuarioId))
        {
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario del token.");
        }

        return usuarioId;
    }
}
