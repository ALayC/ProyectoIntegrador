namespace ProyectoIntegrador.Data.Entities;

public class Auditoria
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? DatosAnteriores { get; set; } // JSON serializado
    public string? DatosNuevos { get; set; } // JSON serializado

    // Navegación
    public Usuario Usuario { get; set; } = null!;
}
