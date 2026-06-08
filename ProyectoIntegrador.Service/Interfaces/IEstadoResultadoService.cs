using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces
{
    public interface IEstadoResultadoService
    {
        Task<EstadoResultadosResponseDto> Generar(EstadoResultadosFiltroDto filtro);
    }
}
