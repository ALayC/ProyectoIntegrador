using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces
{
    public interface IEstadoResultadoService
    {
        Task<EstadoResultadosResponseDto> GenerarEstadoResultados(EstadoResultadosFiltroDto filtro);
    }
}
