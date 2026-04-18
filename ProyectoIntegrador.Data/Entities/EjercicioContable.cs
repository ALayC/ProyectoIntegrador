namespace ProyectoIntegrador.Data.Entities;

public class EjercicioContable
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty; // Abierto, Cerrado

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public ICollection<AsientoContable> Asientos { get; set; } = new List<AsientoContable>();
    public ICollection<SaldoCuenta> SaldosCuenta { get; set; } = new List<SaldoCuenta>();
}
