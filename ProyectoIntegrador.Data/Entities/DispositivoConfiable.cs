namespace ProyectoIntegrador.Data.Entities;

public class DispositivoConfiable
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime CreadoEn { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
}
