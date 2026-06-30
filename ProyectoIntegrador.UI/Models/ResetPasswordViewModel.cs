using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "El token es obligatorio.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContraseña { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContraseña { get; set; } = string.Empty;

    // Para mostrar mensajes de error/éxito en la UI
    public string? Mensaje { get; set; }
    public bool EsExito { get; set; }
}
