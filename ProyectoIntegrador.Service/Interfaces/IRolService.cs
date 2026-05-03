using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IRolService
{
    Task<List<RolResponseDto>> ObtenerTodos();
    Task<RolResponseDto> ObtenerPorId(Guid id);
    Task<RolResponseDto> Crear(CrearRolDto dto);
    Task<RolResponseDto> Actualizar(Guid id, CrearRolDto dto);
    Task AsignarPermiso(Guid rolId, Guid permisoId);
    Task RemoverPermiso(Guid rolId, Guid permisoId);
}
