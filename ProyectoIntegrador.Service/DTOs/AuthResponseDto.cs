namespace ProyectoIntegrador.Service.DTOs;

public class AuthResponseDto
{
    public string? Token { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    // 2FA
    public bool Requires2FA { get; set; } = false;
    public string? TempToken { get; set; }
}
