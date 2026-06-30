using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO para solicitar restablecimiento de contraseña (olvidé contraseña).
/// </summary>
public class SolicitarResetContraseñaDto
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// DTO para restablecer la contraseña usando el token de email.
/// </summary>
public class RestablecerContraseñaDto
{
    [Required(ErrorMessage = "El token es obligatorio.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string NuevaContraseña { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
    [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContraseña { get; set; } = string.Empty;
}
