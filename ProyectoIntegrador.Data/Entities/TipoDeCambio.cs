namespace ProyectoIntegrador.Data.Entities;

public class TipoDeCambio
{
    public Guid Id { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Valor { get; set; }
    public string FuenteOrigen { get; set; } = string.Empty; // BCU, Manual
}
