using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IPermisoService
{
    Task<List<PermisoResponseDto>> ObtenerTodos();
    Task<List<PermisoResponseDto>> ObtenerPorModulo(string modulo);
    Task<List<PermisoResponseDto>> ObtenerPorRol(Guid rolId);
}
