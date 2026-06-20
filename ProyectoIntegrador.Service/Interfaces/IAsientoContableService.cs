using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IAsientoContableService
{
    Task<AsientoContableDto> Crear(CrearAsientoContableDto dto, Guid usuarioId);
    Task<AsientoContableDto> ObtenerPorId(Guid id);
    Task<(List<AsientoContableResumenDto> Items, int Total)> Listar(FiltroAsientoDto filtro);
    Task<AsientoContableDto> Revertir(Guid asientoId, Guid usuarioId);
    Task<ResultadoImportacionBulkDto> ImportarBulk(ImportarAsientosBulkDto dto, Guid usuarioId);
}