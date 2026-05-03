namespace ProyectoIntegrador.Service.DTOs;

/// <summary>Request para crear un rol custom.</summary>
public class CrearRolDto
{
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Response con datos de un rol y sus permisos asignados.</summary>
public class RolResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsPredefinido { get; set; }
    public List<PermisoResponseDto> Permisos { get; set; } = [];
}

/// <summary>Request para asignar o remover un permiso a un rol.</summary>
public class AsignarPermisoDto
{
    public Guid PermisoId { get; set; }
}
