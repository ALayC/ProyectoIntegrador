using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
    }

    public async Task<List<UsuarioResponseDto>> ObtenerTodos()
    {
        var usuarios = await _usuarioRepository.ObtenerTodos();
        return usuarios.Select(Mapear).ToList();
    }

    public async Task<UsuarioResponseDto> ObtenerPorId(Guid id)
    {
        var usuario = await _usuarioRepository.ObtenerPorId(id)
   ?? throw new EntidadNoEncontradaException("Usuario", id);
        return Mapear(usuario);
    }

    public async Task<UsuarioResponseDto> Crear(CrearUsuarioDto dto)
    {
        // Validar email único
        if (await _usuarioRepository.ExisteEmail(dto.Email))
            throw new DuplicadoException("email", dto.Email);

        // Validar que el rol exista
        var rol = await _rolRepository.ObtenerPorId(dto.RolId)
    ?? throw new EntidadNoEncontradaException("Rol", dto.RolId);

        // Validar regla de negocio: ContadorId solo para Auxiliar Contable
        await ValidarContadorId(dto.RolId, dto.ContadorId);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena, workFactor: 12),
            NombreCompleto = dto.NombreCompleto,
            ProveedorAuth = "Local",
            Estado = "Activo",
            RolId = dto.RolId,
            ContadorId = dto.ContadorId,
            CreatedAt = DateTime.UtcNow
        };

        await _usuarioRepository.Guardar(usuario);

        // Recargar con Rol y Contador para el response
        usuario.Rol = rol;
        if (dto.ContadorId.HasValue)
        {
            usuario.Contador = await _usuarioRepository.ObtenerPorId(dto.ContadorId.Value);
        }

        return Mapear(usuario);
    }

    public async Task<UsuarioResponseDto> Editar(Guid id, EditarUsuarioDto dto, Guid adminId)
    {
        if (id == adminId)
            throw new AccesoNoAutorizadoException("El administrador no puede editarse a sí mismo.");

        var usuario = await _usuarioRepository.ObtenerPorId(id)
         ?? throw new EntidadNoEncontradaException("Usuario", id);

        // Validar rol
        var rol = await _rolRepository.ObtenerPorId(dto.RolId)
            ?? throw new EntidadNoEncontradaException("Rol", dto.RolId);

        // Validar regla de negocio: ContadorId solo para Auxiliar Contable
        await ValidarContadorId(dto.RolId, dto.ContadorId);

        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.RolId = dto.RolId;
        usuario.ContadorId = dto.ContadorId;

        await _usuarioRepository.Actualizar(usuario);
        usuario.Rol = rol;
        if (dto.ContadorId.HasValue)
            usuario.Contador = await _usuarioRepository.ObtenerPorId(dto.ContadorId.Value);
        else
            usuario.Contador = null;

        return Mapear(usuario);
    }

    public async Task Desactivar(Guid id, Guid adminId)
    {
        // No puede desactivarse a sí mismo
        if (id == adminId)
            throw new AccesoNoAutorizadoException("El administrador no puede desactivarse a sí mismo.");

        var usuario = await _usuarioRepository.ObtenerPorId(id)
 ?? throw new EntidadNoEncontradaException("Usuario", id);

        usuario.Estado = "Inactivo";
        await _usuarioRepository.Actualizar(usuario);
    }

    // ?????????????????????????????????????????????
    // Validaciones privadas
    // ?????????????????????????????????????????????

    private async Task ValidarContadorId(Guid rolId, Guid? contadorId)
    {
        if (rolId == SeedData.RolAuxiliarId)
        {
            // Auxiliar: ContadorId es obligatorio y debe ser un Contador activo
            if (!contadorId.HasValue)
                throw new EntidadNoEncontradaException("ContadorId es obligatorio para el rol Auxiliar Contable.");

            var contador = await _usuarioRepository.ObtenerPorId(contadorId.Value)
    ?? throw new EntidadNoEncontradaException("Contador", contadorId.Value);

            if (contador.RolId != SeedData.RolContadorId)
                throw new AccesoNoAutorizadoException("El ContadorId proporcionado no corresponde a un usuario con rol Contador.");
        }
        else
        {
            // Cualquier otro rol: ContadorId debe ser null
            if (contadorId.HasValue)
                throw new AccesoNoAutorizadoException("El campo ContadorId solo aplica para el rol Auxiliar Contable.");
        }
    }

    // ?????????????????????????????????????????????
    // Mapeo
    // ?????????????????????????????????????????????

    private static UsuarioResponseDto Mapear(Usuario u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        NombreCompleto = u.NombreCompleto,
        Rol = u.Rol?.Nombre ?? string.Empty,
        RolId = u.RolId,
        ContadorAsignado = u.Contador?.NombreCompleto,
        ContadorId = u.ContadorId,
        Estado = u.Estado
    };
}
