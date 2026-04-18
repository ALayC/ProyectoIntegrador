namespace ProyectoIntegrador.Data.Entities;

public class CentroDeCosto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // Activo, Inactivo

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public ICollection<LineaAsiento> LineasAsiento { get; set; } = new List<LineaAsiento>();
}
