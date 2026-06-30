using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string? Mensaje { get; set; }
    public bool EsExito { get; set; }
}
