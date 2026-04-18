namespace ProyectoIntegrador.Data.Entities;

public class Rol
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsPredefinido { get; set; }

    // Navegación
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
