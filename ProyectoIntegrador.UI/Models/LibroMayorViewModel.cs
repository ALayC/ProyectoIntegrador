namespace ProyectoIntegrador.UI.Models;

public class LibroMayorResponseViewModel
{
    public Guid ClienteId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public Guid? EjercicioId { get; set; }
    public List<LibroMayorCuentaViewModel> Cuentas { get; set; } = new();
}

public class LibroMayorMovimientoViewModel
{
    public Guid AsientoId { get; set; }
    public int NumeroAsiento { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public decimal TipoCambio { get; set; }
    public decimal DebeBase { get; set; }
    public decimal HaberBase { get; set; }
    public decimal SaldoAcumulado { get; set; }
    public decimal SaldoAcumuladoBase { get; set; }
}

public class LibroMayorCuentaViewModel
{
    public Guid CuentaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal Debitos { get; set; }
    public decimal Creditos { get; set; }
    public decimal SaldoFinal { get; set; }
    public decimal SaldoInicialBase { get; set; }
    public decimal DebitosBase { get; set; }
    public decimal CreditosBase { get; set; }
    public decimal SaldoFinalBase { get; set; }
    public List<LibroMayorMovimientoViewModel> Movimientos { get; set; } = new();
}

public class LibroMayorViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid? EjercicioId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public List<Guid> CuentaIds { get; set; } = new();
    public List<EjercicioContableViewModel> Ejercicios { get; set; } = new();
    public List<CuentaContableViewModel> Cuentas { get; set; } = new();
    public List<LibroMayorCuentaViewModel> CuentasMayor { get; set; } = new();
}
