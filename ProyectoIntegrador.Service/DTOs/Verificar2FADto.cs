using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

public class Verificar2FADto
{
    [Required(ErrorMessage = "El token temporal es obligatorio.")]
    public string TempToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos.")]
    public string Codigo { get; set; } = string.Empty;

    public bool RecordarDispositivo { get; set; } = false;
}
