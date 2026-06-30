using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UIOptions _uiOptions;

    public AuthController(IAuthService authService, IOptions<UIOptions> uiOptions)
    {
        _authService = authService;
        _uiOptions = uiOptions.Value;
    }

    /// <summary>
    /// Registra un nuevo usuario con rol Contador por defecto.
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("register")]
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

    /// <summary>
    /// Valida un ID Token de Google y devuelve un JWT propio.
    /// </summary>
    [HttpPost("google-login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        Console.WriteLine($"[API recibió] IdToken length: {dto.IdToken?.Length ?? 0}");
        Console.WriteLine($"[API recibió] AccessToken length: {dto.AccessToken?.Length ?? 0}");

        var resultado = await _authService.LoginConGoogle(dto.IdToken, dto.AccessToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Confirma el email del usuario usando el token enviado por email.
    /// Devuelve JWT si es exitoso.
    /// </summary>
    [HttpPost("confirm-email")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> ConfirmarEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { error = "Token de confirmación requerido.", codigo = "TOKEN_REQUERIDO" });
        }

        var resultado = await _authService.ConfirmarEmailAsync(token);
        return Ok(new
        {
            mensaje = "? Email confirmado exitosamente. Puedes iniciar sesión.",
            usuario = resultado
        });
    }

    /// <summary>
    /// Reenvía el email de confirmación si el anterior expiró.
    /// </summary>
    [HttpPost("resend-confirmation-email")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> ReenviarConfirmacionEmail([FromBody] ReenviarEmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { error = "Email requerido.", codigo = "EMAIL_REQUERIDO" });
        }

        // Usar baseUrl de configuración (apunta a UI)
        var baseUrl = _uiOptions.BaseUrl;

        await _authService.ReenviarConfirmacionEmailAsync(dto.Email, baseUrl);
        return Ok(new
        {
            mensaje = "? Email de confirmación reenviado. Revisa tu bandeja de entrada."
        });
    }

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña.
    /// Envía email con link de restablecimiento.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> OlvidéContraseña([FromBody] SolicitarResetContraseñaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { error = "Email requerido.", codigo = "EMAIL_REQUERIDO" });
        }

        // Usar baseUrl de configuración (apunta a UI)
        var baseUrl = _uiOptions.BaseUrl;

        await _authService.SolicitarRestablecimientoContraseñaAsync(dto.Email, baseUrl);
        return Ok(new
        {
            mensaje = "? Email de restablecimiento enviado. Revisa tu bandeja de entrada."
        });
    }

    /// <summary>
    /// Restablece la contraseña del usuario usando el token enviado por email.
    /// Devuelve JWT si es exitoso (auto-login).
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> RestablecerContraseña([FromBody] RestablecerContraseñaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
        {
            return BadRequest(new { error = "Token requerido.", codigo = "TOKEN_REQUERIDO" });
        }

        var resultado = await _authService.RestablecerContraseñaAsync(dto.Token, dto.NuevaContraseña);
        return Ok(new
        {
            mensaje = "? Contraseña actualizada exitosamente.",
            usuario = resultado
        });
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
