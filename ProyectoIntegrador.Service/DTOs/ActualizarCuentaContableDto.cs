using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de entrada para actualizar una cuenta contable.
/// </summary>
public class ActualizarCuentaContableDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La naturaleza es obligatoria.")]
    public string Naturaleza { get; set; } = string.Empty;

    public bool EsImputable { get; set; }

    public string? Estado { get; set; }

    public Guid? CuentaPadreId { get; set; }
}
