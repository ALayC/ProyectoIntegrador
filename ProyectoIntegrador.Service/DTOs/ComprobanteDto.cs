namespace ProyectoIntegrador.Service.DTOs;

public class ComprobanteCrearDto
{
    public Guid ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ImporteNeto { get; set; }
    public decimal TasaIVA { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal ImporteTotal { get; set; }
}

public class ComprobanteModificarDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ImporteNeto { get; set; }
    public decimal TasaIVA { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal ImporteTotal { get; set; }
}

public class GenerarAsientoDesdeComprobanteDto
{
    public Guid EjercicioId { get; set; }
    public Guid CuentaDebeId { get; set; }
    public Guid CuentaHaberId { get; set; }
    public DateOnly? Fecha { get; set; }
    public string? Glosa { get; set; }
}

public class ComprobanteResumenDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? AsientoId { get; set; }
}

public class ComprobanteDetalleDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ImporteNeto { get; set; }
    public decimal TasaIVA { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? AsientoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class FiltroComprobanteDto
{
    public Guid ClienteId { get; set; }
    public string? Tipo { get; set; }
    public string? RUT { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 20;
}
