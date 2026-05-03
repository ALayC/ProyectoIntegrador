namespace ProyectoIntegrador.Service.DTOs;

/// <summary>Response de un permiso individual.</summary>
public class PermisoResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
}
