namespace ProyectoIntegrador.Data.Entities;

public class PlanDeCuentas
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public ICollection<CuentaContable> CuentasContables { get; set; } = new List<CuentaContable>();
}
