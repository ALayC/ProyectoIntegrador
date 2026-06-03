namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// Filtros de consulta para el Libro Mayor.
/// </summary>
public class LibroMayorFiltroDto
{
    public Guid ClienteId { get; set; }
    public List<Guid>? CuentaIds { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public Guid? EjercicioId { get; set; }
}

/// <summary>
/// Movimiento individual dentro del Libro Mayor.
/// </summary>
public class LibroMayorMovimientoDto
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

/// <summary>
/// Resumen del mayor por cuenta con sus movimientos y saldos.
/// </summary>
public class LibroMayorCuentaDto
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
    public List<LibroMayorMovimientoDto> Movimientos { get; set; } = new();
}

/// <summary>
/// Respuesta de Libro Mayor para un cliente y período.
/// </summary>
public class LibroMayorResponseDto
{
    public Guid ClienteId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public Guid? EjercicioId { get; set; }
    public List<LibroMayorCuentaDto> Cuentas { get; set; } = new();
}
