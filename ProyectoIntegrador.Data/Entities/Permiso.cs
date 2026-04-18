namespace ProyectoIntegrador.Data.Entities;

public class Permiso
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;

    // Navegación
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
