namespace ProyectoIntegrador.Data.Entities;

public class EjercicioContable
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty; // Abierto, Cerrado

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public ICollection<AsientoContable> Asientos { get; set; } = new List<AsientoContable>();
    public ICollection<SaldoCuenta> SaldosCuenta { get; set; } = new List<SaldoCuenta>();
}
