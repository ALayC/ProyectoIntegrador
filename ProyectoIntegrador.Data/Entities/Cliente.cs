namespace ProyectoIntegrador.Data.Entities;

public class Cliente
{
    public Guid Id { get; set; }
    public Guid ContadorId { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoContribuyente { get; set; } = string.Empty; // CEDE, ResponsableIVA, Monotributo, LiteralE, NoAlcanzado, Exento
    public string MonedaBase { get; set; } = string.Empty; // UYU, USD
    public string Estado { get; set; } = string.Empty; // Activo, Inactivo

    // Navegación
    public Usuario Contador { get; set; } = null!;
    public PlanDeCuentas? PlanDeCuentas { get; set; }
    public ICollection<AsientoContable> Asientos { get; set; } = new List<AsientoContable>();
    public ICollection<EjercicioContable> Ejercicios { get; set; } = new List<EjercicioContable>();
    public ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    public ICollection<CentroDeCosto> CentrosDeCosto { get; set; } = new List<CentroDeCosto>();
    public ICollection<Importacion> Importaciones { get; set; } = new List<Importacion>();
    public ICollection<SaldoCuenta> SaldosCuenta { get; set; } = new List<SaldoCuenta>();
}
