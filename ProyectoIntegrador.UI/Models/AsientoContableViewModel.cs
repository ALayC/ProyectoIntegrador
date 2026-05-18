using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class LineaAsientoInputViewModel
{
    [Required]
    public Guid CuentaContableId { get; set; }
    public Guid? CentroCostoId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El importe debe ser mayor o igual a cero.")]
    public decimal Debe { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El importe debe ser mayor o igual a cero.")]
    public decimal Haber { get; set; }

    public string Moneda { get; set; } = "UYU";
    public decimal TipoCambio { get; set; } = 1m;
}

public class CrearAsientoViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ejercicio es obligatorio.")]
    public Guid EjercicioId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [Display(Name = "Fecha")]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La glosa es obligatoria.")]
    [MaxLength(500)]
    [Display(Name = "Glosa / Descripción")]
    public string Glosa { get; set; } = string.Empty;

    public List<LineaAsientoInputViewModel> Lineas { get; set; } = new()
    {
        new LineaAsientoInputViewModel(),
        new LineaAsientoInputViewModel()
    };

    // Datos para los selects del formulario
    public List<EjercicioContableViewModel> Ejercicios { get; set; } = new();
    public List<CuentaContableViewModel> Cuentas { get; set; } = new();
}

public class LineaAsientoViewModel
{
    public Guid Id { get; set; }
    public Guid CuentaContableId { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string NombreCuenta { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = string.Empty;
}

public class AsientoContableViewModel
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid EjercicioId { get; set; }
    public Guid? AsientoOrigenId { get; set; }
    public List<LineaAsientoViewModel> Lineas { get; set; } = new();
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
}

public class AsientoContableResumenViewModel
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
}

public class LibroDiarioIndexViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid? EjercicioId { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public List<EjercicioContableViewModel> Ejercicios { get; set; } = new();
    public List<AsientoContableResumenViewModel> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 20;
    public int TotalPaginas => (int)Math.Ceiling((double)Total / CantidadPorPagina);
}