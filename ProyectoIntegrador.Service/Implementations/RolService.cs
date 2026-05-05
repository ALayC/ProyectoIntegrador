using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;

    public RolService(IRolRepository rolRepository, IPermisoRepository permisoRepository)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
    }

    public async Task<List<RolResponseDto>> ObtenerTodos()
    {
        var roles = await _rolRepository.ObtenerTodos();
        var result = new List<RolResponseDto>();
        foreach (var rol in roles)
        {
            var permisos = await _rolRepository.ObtenerPermisos(rol.Id);
            result.Add(MapearRol(rol, permisos));
        }
        return result;
    }

    public async Task<RolResponseDto> ObtenerPorId(Guid id)
    {
        var rol = await _rolRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Rol", id);

        var permisos = await _rolRepository.ObtenerPermisos(id);
        return MapearRol(rol, permisos);
    }

    public async Task<RolResponseDto> Crear(CrearRolDto dto)
    {
        var existe = await _rolRepository.ObtenerPorNombre(dto.Nombre);
        if (existe is not null)
            throw new DuplicadoException("Nombre", dto.Nombre);

        var rol = new Rol
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            EsPredefinido = false
        };

        await _rolRepository.Guardar(rol);
        return MapearRol(rol, []);
    }

    public async Task<RolResponseDto> Actualizar(Guid id, CrearRolDto dto)
    {
        var rol = await _rolRepository.ObtenerPorId(id)
       ?? throw new EntidadNoEncontradaException("Rol", id);

        if (rol.EsPredefinido)
            throw new AccesoNoAutorizadoException("Los roles predefinidos no pueden modificarse.");

        var existe = await _rolRepository.ObtenerPorNombre(dto.Nombre);
        if (existe is not null && existe.Id != id)
            throw new DuplicadoException("Nombre", dto.Nombre);

        rol.Nombre = dto.Nombre;
        await _rolRepository.Actualizar(rol);

        var permisos = await _rolRepository.ObtenerPermisos(id);
        return MapearRol(rol, permisos);
    }

    public async Task AsignarPermiso(Guid rolId, Guid permisoId)
    {
        var rol = await _rolRepository.ObtenerPorId(rolId)
            ?? throw new EntidadNoEncontradaException("Rol", rolId);

        var permiso = await _permisoRepository.ObtenerPorId(permisoId)
       ?? throw new EntidadNoEncontradaException("Permiso", permisoId);

        await _rolRepository.AsignarPermiso(rolId, permisoId);
    }

    public async Task RemoverPermiso(Guid rolId, Guid permisoId)
    {
        var rol = await _rolRepository.ObtenerPorId(rolId)
            ?? throw new EntidadNoEncontradaException("Rol", rolId);

        var permiso = await _permisoRepository.ObtenerPorId(permisoId)
   ?? throw new EntidadNoEncontradaException("Permiso", permisoId);

        await _rolRepository.RemoverPermiso(rolId, permisoId);
    }

    // ?????????????????????????????????????????????
    private static RolResponseDto MapearRol(Rol rol, IEnumerable<Data.Entities.Permiso> permisos)
        => new()
        {
            Id = rol.Id,
            Nombre = rol.Nombre,
            EsPredefinido = rol.EsPredefinido,
            Permisos = permisos.Select(p => new PermisoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Modulo = p.Modulo,
                Accion = p.Accion
            }).ToList()
        };
}
