using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra un nuevo usuario con rol Contador por defecto.
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Register([FromBody] RegistroDto registroDto)
    {
        var resultado = await _authService.Registrar(registroDto);
        return Created(string.Empty, resultado);
    }

    /// <summary>
    /// Autentica un usuario y devuelve un JWT válido por 1 hora.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var resultado = await _authService.Login(loginDto);
        return Ok(resultado);
    }

    /// <summary>
    /// Invalida el token actual del usuario (logout).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var token = ObtenerTokenDelHeader();

        await _authService.Logout(usuarioId, token);
        return Ok(new { mensaje = "Sesión cerrada exitosamente." });
    }

    /// <summary>
    /// Devuelve los datos del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var usuarioId = ObtenerUsuarioIdDelToken();
        var resultado = await _authService.ObtenerUsuarioActual(usuarioId);
        return Ok(resultado);
    }

    // ??????????????????????????????????????????????
    // Helpers privados
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

    private string ObtenerTokenDelHeader()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("No se encontró el token en el header Authorization.");
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}
