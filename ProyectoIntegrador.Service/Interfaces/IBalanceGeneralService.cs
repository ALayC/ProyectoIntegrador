using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces
{
    public interface IBalanceGeneralService
    {
        Task<BalanceGeneralResponseDto> Generar(BalanceGeneralFiltroDto filtro);
    }
}
