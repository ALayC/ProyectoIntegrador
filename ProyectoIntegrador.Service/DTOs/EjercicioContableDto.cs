namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta para ejercicios contables.
/// </summary>
public class EjercicioContableDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}
