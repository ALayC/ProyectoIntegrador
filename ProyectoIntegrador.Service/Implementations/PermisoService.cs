using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class PermisoService : IPermisoService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly ILogger<PermisoService> _logger;

    public PermisoService(IPermisoRepository permisoRepository, ILogger<PermisoService> logger)
    {
        _permisoRepository = permisoRepository;
        _logger = logger;
    }

    public async Task<List<PermisoResponseDto>> ObtenerTodos()
    {
        var permisos = await _permisoRepository.ObtenerTodos();
        _logger.LogInformation("Permisos consultados | Total: {Total}", permisos.Count);
        return permisos.Select(Mapear).ToList();
    }

    public async Task<List<PermisoResponseDto>> ObtenerPorModulo(string modulo)
    {
        var permisos = await _permisoRepository.ObtenerPorModulo(modulo);
        _logger.LogInformation("Permisos consultados por modulo | Modulo: {Modulo} | Total: {Total}", modulo, permisos.Count);
        return permisos.Select(Mapear).ToList();
    }

    public async Task<List<PermisoResponseDto>> ObtenerPorRol(Guid rolId)
    {
        var permisos = await _permisoRepository.ObtenerPorRol(rolId);
        _logger.LogInformation("Permisos consultados por rol | RolId: {RolId} | Total: {Total}", rolId, permisos.Count);
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
