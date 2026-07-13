namespace ProyectoIntegrador.Service.DTOs;

public class InvitarAuxiliarDto
{
    public string Email { get; set; } = string.Empty;
}

public class InvitacionAuxiliarResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
}
