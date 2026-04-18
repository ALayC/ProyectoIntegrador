namespace ProyectoIntegrador.Data.Entities;

public class AsientoContable
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid EjercicioId { get; set; }
    public Guid? AsientoOrigenId { get; set; }
    public int Numero { get; set; }
    public DateTime Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // Confirmado, Revertido

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public EjercicioContable Ejercicio { get; set; } = null!;
    public AsientoContable? AsientoOrigen { get; set; }
    public ICollection<AsientoContable> AsientosReversion { get; set; } = new List<AsientoContable>();
    public ICollection<LineaAsiento> LineasAsiento { get; set; } = new List<LineaAsiento>();
    public ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
}
