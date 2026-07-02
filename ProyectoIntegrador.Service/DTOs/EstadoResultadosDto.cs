namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// Filtros de consulta para generar el Estado de Resultados.
/// </summary>

public class EstadoResultadosFiltroDto
{
    public Guid ClienteId { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly FechaHasta { get; set; }
}

/// <summary>
/// Respuesta del Estado de Resultados para un cliente y período.
/// </summary>
public class EstadoResultadosResponseDto
{
    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal ResultadoNeto { get; set; }

    public List<EstadoResultadoNodoDto> Ingresos { get; set; } = new();

    public List<EstadoResultadoNodoDto> Egresos { get; set; } = new();
}

/// <summary>
/// Nodo del árbol del Estado de Resultados.
/// Representa una cuenta contable y sus subtotales acumulados.
/// </summary>
public class EstadoResultadoNodoDto
{
    public Guid CuentaId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Importe { get; set; }

    public List<EstadoResultadoNodoDto> Hijas { get; set; } = new();
}