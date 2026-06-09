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