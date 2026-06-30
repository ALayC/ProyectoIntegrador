namespace ProyectoIntegrador.UI.Models;

public class BalanceGeneralViewModel
{
    public Guid ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public DateOnly? FechaHasta { get; set; }

    public BalanceGeneralResponseViewModel? Resultado { get; set; }
}

public class BalanceGeneralResponseViewModel
{
    public decimal TotalActivo { get; set; }

    public decimal TotalPasivo { get; set; }

    public decimal TotalPatrimonio { get; set; }

    public decimal TotalPasivoPatrimonio { get; set; }

    public bool Balancea { get; set; }

    public List<BalanceGeneralNodoViewModel> Activos { get; set; } = new();

    public List<BalanceGeneralNodoViewModel> Pasivos { get; set; } = new();

    public List<BalanceGeneralNodoViewModel> Patrimonio { get; set; } = new();
}

public class BalanceGeneralNodoViewModel
{
    public Guid CuentaId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Saldo { get; set; }

    public List<BalanceGeneralNodoViewModel> Hijas { get; set; } = new();
}