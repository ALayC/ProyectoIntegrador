using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> Registrar(RegistroDto registroDto);
    Task<AuthResponseDto> Login(LoginDto loginDto);
    Task Logout(Guid usuarioId, string token);
    Task<AuthResponseDto> ObtenerUsuarioActual(Guid id);
    Task<AuthResponseDto> LoginConGoogle(string? idToken, string? accessToken);

    /// <summary>
    /// Confirma el email del usuario usando el token generado al registrarse.
    /// </summary>
    Task<AuthResponseDto> ConfirmarEmailAsync(string token);

    /// <summary>
    /// Reenvía email de confirmación si el anterior expiró.
    /// </summary>
    Task ReenviarConfirmacionEmailAsync(string email, string baseUrl);

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña.
    /// Envía email con link de restablecimiento.
    /// </summary>
    Task SolicitarRestablecimientoContraseñaAsync(string email, string baseUrl);

    /// <summary>
    /// Restablece la contraseña del usuario usando el token enviado por email.
    /// </summary>
    Task<AuthResponseDto> RestablecerContraseñaAsync(string token, string nuevaContraseña);

    /// <summary>
    /// Verifica el código 2FA y emite el JWT real. Opcionalmente registra dispositivo confiable.
    /// </summary>
    Task<AuthResponseDto> Verificar2FAAsync(Verificar2FADto dto, string? tokenDispositivoActual);

    /// <summary>
    /// Genera un token de dispositivo confiable para omitir 2FA por 7 días.
    /// </summary>
    Task<string> GenerarTokenDispositivoConfiableAsync(Guid usuarioId);
}
