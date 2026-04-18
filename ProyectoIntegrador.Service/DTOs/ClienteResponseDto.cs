namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta con los datos completos de un cliente.
/// </summary>
public class ClienteResponseDto
{
    public Guid Id { get; set; }
    public Guid ContadorId { get; set; }
    public string Rut { get; set; } = string.Empty;
  public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoContribuyente { get; set; } = string.Empty;
    public string MonedaBase { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
