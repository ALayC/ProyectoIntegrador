namespace ProyectoIntegrador.Service.DTOs;

public class LiquidacionIvaFiltroDto
{
    public Guid ClienteId { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
}

public class LiquidacionIvaResponseDto
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal TotalIvaVentas { get; set; }
    public decimal TotalIvaCompras { get; set; }
    public decimal SaldoNeto { get; set; }

    /// <summary>"APagar" si SaldoNeto > 0; "AFavor" si SaldoNeto &lt;= 0.</summary>
    public string TipoSaldo { get; set; } = string.Empty;
}
