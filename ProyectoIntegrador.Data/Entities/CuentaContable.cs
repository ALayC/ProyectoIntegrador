namespace ProyectoIntegrador.Data.Entities;

public class CuentaContable
{
    public Guid Id { get; set; }
    public Guid PlanCuentasId { get; set; }
    public Guid? CuentaPadreId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Activo, Pasivo, Patrimonio, Ingreso, Egreso
    public string Naturaleza { get; set; } = string.Empty; // Deudora, Acreedora
    public bool EsImputable { get; set; }
    public string Estado { get; set; } = string.Empty; // Activa, Inactiva

    // Navegación
    public PlanDeCuentas PlanDeCuentas { get; set; } = null!;
    public CuentaContable? CuentaPadre { get; set; }
    public ICollection<CuentaContable> CuentasHijas { get; set; } = new List<CuentaContable>();
    public ICollection<LineaAsiento> LineasAsiento { get; set; } = new List<LineaAsiento>();
    public ICollection<SaldoCuenta> SaldosCuenta { get; set; } = new List<SaldoCuenta>();
}
