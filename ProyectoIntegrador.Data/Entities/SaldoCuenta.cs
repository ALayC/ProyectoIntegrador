namespace ProyectoIntegrador.Data.Entities;

public class SaldoCuenta
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid CuentaContableId { get; set; }
    public Guid EjercicioId { get; set; }
    public DateOnly Periodo { get; set; }
    public decimal DebeAcumulado { get; set; }
    public decimal HaberAcumulado { get; set; }
    public decimal Saldo { get; set; }

    // Moneda base (multimoneda)
    public string Moneda { get; set; } = "UYU";
    public decimal DebeAcumuladoBase { get; set; }
    public decimal HaberAcumuladoBase { get; set; }
    public decimal SaldoBase { get; set; }

    // Navegacion
    public Cliente Cliente { get; set; } = null!;
    public CuentaContable CuentaContable { get; set; } = null!;
    public EjercicioContable Ejercicio { get; set; } = null!;
}
