namespace ProyectoIntegrador.Data.Entities;

public class TokenRevocado
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
}
