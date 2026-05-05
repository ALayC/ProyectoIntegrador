namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta para cuentas contables.
/// </summary>
public class CuentaContableDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public bool EsImputable { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? CuentaPadreId { get; set; }
}
