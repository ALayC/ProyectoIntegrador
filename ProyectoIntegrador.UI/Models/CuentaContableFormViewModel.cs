namespace ProyectoIntegrador.UI.Models;

public class CuentaContableFormViewModel
{
    public CuentaContableViewModel Cuenta { get; set; } = new();
    public CuentaContableResumenViewModel? CuentaPadre { get; set; }
}
