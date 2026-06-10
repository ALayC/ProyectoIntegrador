using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers
{
    [ApiController]
    [Route("api/estado-resultados")]
    [Authorize]
    public class EstadoResultadosController : ControllerBase
    {
        private readonly IEstadoResultadosService _EstadoResultadosService;

        public EstadoResultadosController(IEstadoResultadosService service) => _EstadoResultadosService = service;

        /// <summary>Genera un estado de resultados de un rango de fechas dado.</summary>
        [HttpGet]
        [RequierePermiso("Reportes", "Consultar")]
        public async Task<IActionResult> Generar([FromQuery] Guid clienteId, [FromQuery] DateOnly fechaDesde,
            [FromQuery] DateOnly fechaHasta)
        {
            var resultado = await _EstadoResultadosService.Generar(
                new EstadoResultadosFiltroDto
                {
                    ClienteId = clienteId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
                });

            return Ok(resultado);
        }
    }
}
