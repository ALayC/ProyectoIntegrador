using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class Verify2FAViewModel
{
    [Required(ErrorMessage = "El token temporal es obligatorio.")]
    public string TempToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener exactamente 6 dígitos.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe contener solo dígitos.")]
    public string Codigo { get; set; } = string.Empty;

    public bool RecordarDispositivo { get; set; } = false;
}
