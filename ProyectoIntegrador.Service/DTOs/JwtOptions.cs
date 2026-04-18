namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// Opciones de configuración JWT inyectadas desde appsettings.json.
/// </summary>
public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; } = 60;
}
