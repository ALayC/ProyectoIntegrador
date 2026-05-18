namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta para un registro de auditoría.
/// </summary>
public class AuditoriaResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
}