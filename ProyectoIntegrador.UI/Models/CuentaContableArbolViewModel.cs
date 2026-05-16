namespace ProyectoIntegrador.UI.Models;

public class CuentaContableArbolViewModel
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsImputable { get; set; }
    public bool EsSistema { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<CuentaContableArbolViewModel> Hijas { get; set; } = new();
    public Guid PlanCuentasId { get; set; }
    public Guid CuentaPadreId { get; set; }
}
