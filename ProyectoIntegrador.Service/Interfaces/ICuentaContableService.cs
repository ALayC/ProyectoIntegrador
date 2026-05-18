using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface ICuentaContableService
{
    Task<CuentaContableDto> Crear(Guid planCuentasId, CrearCuentaContableDto dto, Guid usuarioId);
    Task<CuentaContableDto> ObtenerPorId(Guid id);
    Task<PaginadoDto<CuentaContableDto>> ObtenerPorPlanPaginado(Guid planCuentasId, int pagina, int cantidadPorPagina);
    Task<List<CuentaContableArbolDto>> ObtenerArbolDeCuentas(Guid planId);
    Task<CuentaContableDto> Actualizar(Guid id, ActualizarCuentaContableDto dto, Guid usuarioId);
    Task Desactivar(Guid id, Guid usuarioId);
    Task Activar(Guid id);
}
