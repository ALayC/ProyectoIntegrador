using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class AuxiliarViewModel
{
    public List<InvitacionAuxiliarViewModel> Invitaciones { get; set; } = [];
    public List<AuxiliarActivoViewModel> Auxiliares { get; set; } = [];
    public InvitarAuxiliarViewModel FormInvitar { get; set; } = new();
}

public class InvitarAuxiliarViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;
}

public class InvitacionAuxiliarViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
}

public class AuxiliarActivoViewModel
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

// DTOs para deserializar respuestas de la API
public class InvitacionAuxiliarApiDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
}

public class AuxiliarActivoApiDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
