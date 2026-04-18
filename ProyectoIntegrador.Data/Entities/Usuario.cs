namespace ProyectoIntegrador.Data.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string ProveedorAuth { get; set; } = string.Empty; // Local, Google
    public string Estado { get; set; } = string.Empty; // Activo, Inactivo
    public Guid RolId { get; set; }
    public Guid? ContadorId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navegación
    public Rol Rol { get; set; } = null!;
    public Usuario? Contador { get; set; }
    public ICollection<Usuario> Auxiliares { get; set; } = new List<Usuario>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public ICollection<AsientoContable> Asientos { get; set; } = new List<AsientoContable>();
    public ICollection<Importacion> Importaciones { get; set; } = new List<Importacion>();
    public ICollection<TokenRevocado> TokensRevocados { get; set; } = new List<TokenRevocado>();
    public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
}
