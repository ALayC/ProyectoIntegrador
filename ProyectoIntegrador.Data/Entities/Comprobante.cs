namespace ProyectoIntegrador.Data.Entities;

public class Comprobante
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? ImportacionId { get; set; }
    public Guid? AsientoId { get; set; }
    public string Tipo { get; set; } = string.Empty; // Compra, Venta
    public string Numero { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaContable { get; set; }
    public string RutContraparte { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty; // UYU, USD
    public decimal ImporteNeto { get; set; }
    public decimal TasaIva { get; set; }
    public decimal ImporteIva { get; set; }

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Importacion? Importacion { get; set; }
    public AsientoContable? Asiento { get; set; }
}
