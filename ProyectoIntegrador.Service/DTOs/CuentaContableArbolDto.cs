namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO para representar cuentas contables en estructura de árbol.
/// </summary>
public class CuentaContableArbolDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsSistema { get; set; }
    public bool EsImputable { get; set; }
    public string Estado { get; set; } = string.Empty;

    public List<CuentaContableArbolDto> Hijas { get; set; } = new();
}
