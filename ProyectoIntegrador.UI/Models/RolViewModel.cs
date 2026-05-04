namespace ProyectoIntegrador.UI.Models;

/// <summary>ViewModel para el listado de roles con permisos agrupados.</summary>
public class RolIndexViewModel
{
    public List<RolItemViewModel> Roles { get; set; } = [];
}

public class RolItemViewModel
{
 public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsPredefinido { get; set; }
    public List<PermisoItemViewModel> Permisos { get; set; } = [];
}

public class PermisoItemViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public bool Asignado { get; set; }
}

/// <summary>ViewModel para crear un rol custom.</summary>
public class CrearRolViewModel
{
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>ViewModel para gestionar permisos de un rol.</summary>
public class GestionPermisosViewModel
{
    public Guid RolId { get; set; }
    public string NombreRol { get; set; } = string.Empty;
public bool EsPredefinido { get; set; }
    public List<ModuloPermisosViewModel> Modulos { get; set; } = [];
}

public class ModuloPermisosViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public List<PermisoCheckViewModel> Permisos { get; set; } = [];
}

public class PermisoCheckViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public bool Asignado { get; set; }
}
