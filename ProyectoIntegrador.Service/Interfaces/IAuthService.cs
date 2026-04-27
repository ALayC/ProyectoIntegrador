using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> Registrar(RegistroDto registroDto);
    Task<AuthResponseDto> Login(LoginDto loginDto);
    Task Logout(Guid usuarioId, string token);
    Task<AuthResponseDto> ObtenerUsuarioActual(Guid id);
    Task<AuthResponseDto> LoginConGoogle(string? idToken, string? accessToken);
}
