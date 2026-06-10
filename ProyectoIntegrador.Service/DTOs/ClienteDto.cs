using System.ComponentModel.DataAnnotations;

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

/// <summary>
/// DTO de entrada para crear o actualizar un cliente.
/// No incluye Id (se genera automáticamente en creación, se recibe por ruta en actualización).
/// </summary>
public class ClienteDto
{
    [Required(ErrorMessage = "El RUT es obligatorio.")]
    public string Rut { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    public string RazonSocial { get; set; } = string.Empty;

    public string? NombreFantasia { get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de contribuyente es obligatorio.")]
    public string TipoContribuyente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La moneda base es obligatoria.")]
    public string MonedaBase { get; set; } = string.Empty;
}