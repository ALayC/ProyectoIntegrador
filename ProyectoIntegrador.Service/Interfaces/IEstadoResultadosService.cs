using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces
{
    public interface IEstadoResultadosService
    {
        Task<EstadoResultadosResponseDto> Generar(EstadoResultadosFiltroDto filtro);
    }
}
