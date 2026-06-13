namespace ProyectoIntegrador.Data.Entities;

public class Comprobante
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public TipoComprobante Tipo { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ImporteNeto { get; set; }
    public decimal TasaIVA { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal ImporteTotal { get; set; }
    public EstadoComprobante Estado { get; set; } = EstadoComprobante.Activo;
    public Guid? AsientoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public AsientoContable? Asiento { get; set; }
}
