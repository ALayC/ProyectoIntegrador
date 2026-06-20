using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface ICuentaContableService
{
    Task<CuentaContableResponseDto> Crear(Guid planCuentasId, CrearCuentaContableDto dto, Guid usuarioId);
    Task<CuentaContableResponseDto> ObtenerPorId(Guid id);
    Task<PaginadoDto<CuentaContableResponseDto>> ObtenerPorPlanPaginado(Guid planCuentasId, int pagina, int cantidadPorPagina);
    Task<List<CuentaContableArbolDto>> ObtenerArbolDeCuentas(Guid planId);
    Task<List<CuentaContableResponseDto>> ObtenerImputables(Guid planCuentasId);
    Task<CuentaContableResponseDto> Actualizar(Guid id, ActualizarCuentaContableDto dto, Guid usuarioId);
    Task Desactivar(Guid id, Guid usuarioId);
    Task Activar(Guid id);
    Task<string> SiguienteCodigoHija(Guid cuentaPadreId);
}
