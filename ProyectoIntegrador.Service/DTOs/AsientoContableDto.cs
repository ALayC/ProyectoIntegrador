namespace ProyectoIntegrador.Service.DTOs;

/// <summary>Input para una línea dentro de un asiento.</summary>
public class LineaAsientoInputDto
{
    public Guid CuentaContableId { get; set; }
    public Guid? CentroCostoId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = "UYU";
    public decimal TipoCambio { get; set; } = 1m;
}

/// <summary>Request para registrar un asiento contable.</summary>
public class CrearAsientoContableDto
{
    public Guid ClienteId { get; set; }
    public Guid EjercicioId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public List<LineaAsientoInputDto> Lineas { get; set; } = new();
}

/// <summary>Response de una línea de asiento.</summary>
public class LineaAsientoDto
{
    public Guid Id { get; set; }
    public Guid CuentaContableId { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string NombreCuenta { get; set; } = string.Empty;
    public Guid? CentroCostoId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public decimal TipoCambio { get; set; }
    public decimal ImporteMonedaBase { get; set; }
}

/// <summary>Response completo de un asiento contable.</summary>
public class AsientoContableDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid EjercicioId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? AsientoOrigenId { get; set; }
    public List<LineaAsientoDto> Lineas { get; set; } = new();
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
}

/// <summary>Response resumido para listados paginados.</summary>
public class AsientoContableResumenDto
{
    public Guid Id { get; set; }
    public Guid EjercicioId { get; set; }
    public int Numero { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
}

/// <summary>Filtros para consulta paginada del Libro Diario.</summary>
public class FiltroAsientoDto
{
    public Guid ClienteId { get; set; }
    public Guid? EjercicioId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 20;
}