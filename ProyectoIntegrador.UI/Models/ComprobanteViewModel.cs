using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class ComprobanteResumenViewModel
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

public class ComprobanteDetalleViewModel
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

    public bool TieneAsiento => AsientoId.HasValue;
    public bool EstaAnulado => string.Equals(Estado, "Anulado", StringComparison.OrdinalIgnoreCase);
}

public class ComprobanteCrearViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = "Factura";

    [Required(ErrorMessage = "El número es obligatorio.")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RUT es obligatorio.")]
    public string RUT { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(0.01, double.MaxValue, ErrorMessage = "El importe neto debe ser mayor a cero.")]
    public decimal ImporteNeto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La tasa IVA no puede ser negativa.")]
    public decimal TasaIVA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El importe IVA no puede ser negativo.")]
    public decimal ImporteIVA { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El importe total debe ser mayor a cero.")]
    public decimal ImporteTotal { get; set; }
}

public class ComprobanteEditarViewModel
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número es obligatorio.")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RUT es obligatorio.")]
    public string RUT { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El importe neto debe ser mayor a cero.")]
    public decimal ImporteNeto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La tasa IVA no puede ser negativa.")]
    public decimal TasaIVA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El importe IVA no puede ser negativo.")]
    public decimal ImporteIVA { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El importe total debe ser mayor a cero.")]
    public decimal ImporteTotal { get; set; }
}

public class ComprobanteIndexViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public string? RUT { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 20;
    public int Total { get; set; }
    public List<ComprobanteResumenViewModel> Items { get; set; } = new();

    public int TotalPaginas => (int)Math.Ceiling((double)Math.Max(Total, 1) / CantidadPorPagina);
}

public class GenerarAsientoDesdeComprobanteViewModel
{
    public Guid ComprobanteId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid EjercicioId { get; set; }
    public Guid CuentaDebeId { get; set; }
    public Guid CuentaHaberId { get; set; }
    public DateOnly? Fecha { get; set; }
    public string? Glosa { get; set; }
}
