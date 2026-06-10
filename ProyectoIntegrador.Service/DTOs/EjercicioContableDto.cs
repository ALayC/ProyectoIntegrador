using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta para ejercicios contables.
/// </summary>
public class EjercicioContableResponseDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}

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

    /// <summary>
    /// Id del usuario que realiza la operación. Lo asigna el controller desde el JWT.
    /// </summary>
    public Guid UsuarioId { get; set; }
}

// <summary>
/// DTO de entrada para actualizar un ejercicio contable.
/// </summary>
public class ActualizarEjercicioContableDto
{
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateOnly? FechaFin { get; set; }
}