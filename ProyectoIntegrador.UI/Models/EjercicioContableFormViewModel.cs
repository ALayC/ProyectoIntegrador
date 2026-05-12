using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

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
