using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class EjercicioContableViewModel
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class EjercicioContableIndexViewModel
{
    public Guid ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public PaginadoViewModel<EjercicioContableViewModel> Paginado { get; set; } = new();
    public bool TieneEjercicioAbierto { get; set; }
}

public class EjercicioContableFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El cliente es obligatorio.")]
    public Guid ClienteId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateOnly? FechaFin { get; set; }
}

public class CierreEjercicioViewModel
{
    public Guid EjercicioId { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal ResultadoNeto { get; set; }
    public int AsientosGenerados { get; set; }
}

public class CierreEjercicioPageViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public List<EjercicioContableViewModel> Ejercicios { get; set; } = new();
    public Guid? EjercicioSeleccionadoId { get; set; }
    public CierreEjercicioViewModel? ResultadoCierre { get; set; }
}

public class LineaAsientoCierreViewModel
{
    public string CodigoCuenta { get; set; } = string.Empty;
    public string NombreCuenta { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string Moneda { get; set; } = string.Empty;
}

public class AsientoCierreViewModel
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public List<LineaAsientoCierreViewModel> Lineas { get; set; } = new();
}

public class ReporteCierreViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public EjercicioContableViewModel Ejercicio { get; set; } = new();
    public List<AsientoCierreViewModel> Asientos { get; set; } = new();
}