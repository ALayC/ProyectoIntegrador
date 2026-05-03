namespace ProyectoIntegrador.Service.DTOs;

/// <summary>Request para que el Admin cree un usuario.</summary>
public class CrearUsuarioDto
{
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public Guid RolId { get; set; }
    public Guid? ContadorId { get; set; }
}

/// <summary>Request para que el Admin edite un usuario.</summary>
public class EditarUsuarioDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public Guid? ContadorId { get; set; }
}

/// <summary>Response con datos de un usuario.</summary>
public class UsuarioResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? ContadorAsignado { get; set; }
    public string Estado { get; set; } = string.Empty;
}
