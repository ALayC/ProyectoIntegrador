namespace ProyectoIntegrador.Data.Entities;

public class Importacion
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid ClienteId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; }
    public int RegistrosImportados { get; set; }
public int RegistrosRechazados { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
}
