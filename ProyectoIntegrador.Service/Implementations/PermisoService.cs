using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class PermisoService : IPermisoService
{
    private readonly IPermisoRepository _permisoRepository;

    public PermisoService(IPermisoRepository permisoRepository)
    {
  _permisoRepository = permisoRepository;
    }

    public async Task<List<PermisoResponseDto>> ObtenerTodos()
    {
    var permisos = await _permisoRepository.ObtenerTodos();
      return permisos.Select(Mapear).ToList();
    }

    public async Task<List<PermisoResponseDto>> ObtenerPorModulo(string modulo)
    {
        var permisos = await _permisoRepository.ObtenerPorModulo(modulo);
 return permisos.Select(Mapear).ToList();
 }

    public async Task<List<PermisoResponseDto>> ObtenerPorRol(Guid rolId)
    {
        var permisos = await _permisoRepository.ObtenerPorRol(rolId);
       return permisos.Select(Mapear).ToList();
    }

    private static PermisoResponseDto Mapear(Data.Entities.Permiso p) => new()
    {
    Id = p.Id,
     Nombre = p.Nombre,
Modulo = p.Modulo,
     Accion = p.Accion
    };
}
