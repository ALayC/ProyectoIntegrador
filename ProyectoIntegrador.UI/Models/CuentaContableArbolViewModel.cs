namespace ProyectoIntegrador.UI.Models;

public class CuentaContableArbolViewModel
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<CuentaContableArbolViewModel> Hijas { get; set; } = new();
}
