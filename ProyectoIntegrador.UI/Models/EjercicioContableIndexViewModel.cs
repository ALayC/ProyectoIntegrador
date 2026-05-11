namespace ProyectoIntegrador.UI.Models;

public class EjercicioContableIndexViewModel
{
    public Guid ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public PaginadoViewModel<EjercicioContableViewModel> Paginado { get; set; } = new();
    public bool TieneEjercicioAbierto { get; set; }
}
