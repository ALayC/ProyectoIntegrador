namespace ProyectoIntegrador.Data.Entities;

public class SaldoCuenta
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid CuentaContableId { get; set; }
    public Guid EjercicioId { get; set; }
    public string Periodo { get; set; } = string.Empty; // Formato "AAAA-MM"
 public decimal DebeAcumulado { get; set; }
    public decimal HaberAcumulado { get; set; }
    public decimal Saldo { get; set; }

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public CuentaContable CuentaContable { get; set; } = null!;
    public EjercicioContable Ejercicio { get; set; } = null!;
}
