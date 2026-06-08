using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoIntegrador.Service.Implementations
{
    public class EstadoResultadosService : IEstadoResultadoService
    {
        private readonly ILineaAsientoRepository _lineaAsientoRepository;

        public EstadoResultadosService(ILineaAsientoRepository lineaAsientoRepository) => _lineaAsientoRepository = lineaAsientoRepository;

        public Task<EstadoResultadosResponseDto> GenerarEstadoResultados(EstadoResultadosFiltroDto filtro)
        {
            throw new NotImplementedException();
        }
    }
}
