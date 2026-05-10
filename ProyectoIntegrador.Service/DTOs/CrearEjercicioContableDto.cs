using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de entrada para crear un ejercicio contable.
/// </summary>
public class CrearEjercicioContableDto
{
    [Required(ErrorMessage = "El cliente es obligatorio.")]
    public Guid? ClienteId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateOnly? FechaFin { get; set; }
}
