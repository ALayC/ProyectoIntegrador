using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de entrada para actualizar un ejercicio contable.
/// </summary>
public class ActualizarEjercicioContableDto
{
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateOnly? FechaFin { get; set; }
}
