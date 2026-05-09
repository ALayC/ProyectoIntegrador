using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IEjercicioContableService
{
    Task<EjercicioContableDto> Crear(CrearEjercicioContableDto dto);
    Task<EjercicioContableDto> ObtenerPorId(Guid id);
    Task<PaginadoDto<EjercicioContableDto>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<EjercicioContableDto> Actualizar(Guid id, ActualizarEjercicioContableDto dto);
    Task Cerrar(Guid id, Guid usuarioId);
}
