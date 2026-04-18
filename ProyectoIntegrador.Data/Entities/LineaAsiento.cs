namespace ProyectoIntegrador.Data.Entities;

public class LineaAsiento
{
    public Guid Id { get; set; }
    public Guid AsientoId { get; set; }
    public Guid CuentaContableId { get; set; }
    public Guid? CentroCostoId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = string.Empty; // UYU, USD
    public decimal TipoCambio { get; set; }
    public decimal ImporteMonedaBase { get; set; }

    // Navegación
    public AsientoContable Asiento { get; set; } = null!;
    public CuentaContable CuentaContable { get; set; } = null!;
    public CentroDeCosto? CentroCosto { get; set; }
}
