using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

/// <summary>
/// ViewModel para la página de confirmación de email.
/// </summary>
public class ConfirmEmailViewModel
{
    public bool EsExito { get; set; }
    public string? Mensaje { get; set; }
}

/// <summary>
/// ViewModel para reenviar email de confirmación.
/// </summary>
public class ResendConfirmationViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;

    public bool EsExito { get; set; }
    public string? Mensaje { get; set; }
}
