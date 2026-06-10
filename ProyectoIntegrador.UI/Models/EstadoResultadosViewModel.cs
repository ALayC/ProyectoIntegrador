namespace ProyectoIntegrador.UI.Models;

public class EstadoResultadosViewModel
{
    public Guid ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public DateOnly? FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public EstadoResultadosResponseViewModel? Resultado { get; set; }
}

public class EstadoResultadosResponseViewModel
{
    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal ResultadoNeto { get; set; }

    public List<EstadoResultadoNodoViewModel> Ingresos { get; set; } = new();

    public List<EstadoResultadoNodoViewModel> Egresos { get; set; } = new();
}

public class EstadoResultadoNodoViewModel
{
    public Guid CuentaId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Importe { get; set; }

    public List<EstadoResultadoNodoViewModel> Hijas { get; set; } = new();
}