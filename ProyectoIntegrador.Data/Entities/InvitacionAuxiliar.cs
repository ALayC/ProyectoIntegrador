namespace ProyectoIntegrador.Data.Entities;

public class InvitacionAuxiliar
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid ContadorId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aceptada, Revocada

    // Navegación
    public Usuario Contador { get; set; } = null!;
}
