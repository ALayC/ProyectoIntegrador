namespace ProyectoIntegrador.UI.Models;

public class LiquidacionIvaViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int? Mes { get; set; }
    public int? Anio { get; set; }
    public LiquidacionIvaResultadoViewModel? Resultado { get; set; }
}

public class LiquidacionIvaResultadoViewModel
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal TotalIvaVentas { get; set; }
    public decimal TotalIvaCompras { get; set; }
    public decimal SaldoNeto { get; set; }
    public string TipoSaldo { get; set; } = string.Empty;
}
