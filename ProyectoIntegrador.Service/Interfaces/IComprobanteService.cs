using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IComprobanteService
{
    Task<ComprobanteDetalleDto> Crear(ComprobanteCrearDto dto, Guid usuarioId);
    Task<ComprobanteDetalleDto> Modificar(Guid id, ComprobanteModificarDto dto, Guid usuarioId);
    Task Anular(Guid id, Guid usuarioId);
    Task<ComprobanteDetalleDto> Obtener(Guid id);
    Task<List<ComprobanteResumenDto>> Listar(FiltroComprobanteDto filtro);
    Task<ComprobanteDetalleDto> ObtenerPorAsiento(Guid asientoId);
    Task<AsientoContableDto> GenerarAsiento(Guid id, GenerarAsientoDesdeComprobanteDto dto, Guid usuarioId);
}
