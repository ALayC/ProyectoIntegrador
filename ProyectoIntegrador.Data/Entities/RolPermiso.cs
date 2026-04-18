namespace ProyectoIntegrador.Data.Entities;

public class RolPermiso
{
    public Guid RolId { get; set; }
    public Guid PermisoId { get; set; }

    // Navegación
    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}
