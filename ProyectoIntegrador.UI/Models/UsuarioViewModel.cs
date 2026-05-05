using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

/// <summary>ViewModel para el listado de usuarios con filtros y paginado.</summary>
public class UsuarioIndexViewModel
{
    public List<UsuarioListItemViewModel> Usuarios { get; set; } = [];
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int CantidadPorPagina { get; set; } = 10;
    public string? FiltroRol { get; set; }
    public string? FiltroEstado { get; set; }
    public bool TienePaginaAnterior => Pagina > 1;
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;
}

public class UsuarioListItemViewModel
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

/// <summary>ViewModel para crear un usuario.</summary>
public class CrearUsuarioViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [Display(Name = "Email")]
  public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

  [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
 public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Display(Name = "Rol")]
    public Guid RolId { get; set; }

    [Display(Name = "Contador asignado")]
    public Guid? ContadorId { get; set; }

    // Para poblar los selects
    public List<RolSelectItem> Roles { get; set; } = [];
    public List<ContadorSelectItem> Contadores { get; set; } = [];
}

/// <summary>ViewModel para editar un usuario.</summary>
public class EditarUsuarioViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Display(Name = "Rol")]
    public Guid RolId { get; set; }

    [Display(Name = "Contador asignado")]
    public Guid? ContadorId { get; set; }

    public string Estado { get; set; } = string.Empty;

public List<RolSelectItem> Roles { get; set; } = [];
    public List<ContadorSelectItem> Contadores { get; set; } = [];
}

public class RolSelectItem
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ContadorSelectItem
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}
