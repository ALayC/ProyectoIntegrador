using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IEjercicioContableService
{
    Task<EjercicioContableResponseDto> Crear(CrearEjercicioContableDto dto);
    Task<EjercicioContableResponseDto> ObtenerPorId(Guid id);
    Task<PaginadoDto<EjercicioContableResponseDto>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<EjercicioContableResponseDto> Actualizar(Guid id, ActualizarEjercicioContableDto dto);
    Task Cerrar(Guid id, Guid usuarioId);
}
