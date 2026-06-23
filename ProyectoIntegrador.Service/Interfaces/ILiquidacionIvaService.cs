using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface ILiquidacionIvaService
{
    Task<LiquidacionIvaResponseDto> Calcular(LiquidacionIvaFiltroDto filtro);
}
