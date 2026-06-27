namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// Opciones de configuración para el servicio de email.
/// Mapea a la sección "Email" en appsettings.json.
/// </summary>
public class EmailOptions
{
    /// <summary>Nombre del remitente (ej: "Sistema Contable").</summary>
    public string RemitenteName { get; set; } = string.Empty;

    /// <summary>Email del remitente (ej: noreply@sistema.local).</summary>
    public string RemitenteMail { get; set; } = string.Empty;

    /// <summary>Host SMTP (ej: smtp.gmail.com, smtp.servidor.com).</summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>Puerto SMTP (típicamente 25, 587, 465).</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>Usuario SMTP para autenticación.</summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>Contraseña SMTP para autenticación.</summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>Usar SSL/TLS en conexión SMTP.</summary>
    public bool UseSsl { get; set; } = true;
}
