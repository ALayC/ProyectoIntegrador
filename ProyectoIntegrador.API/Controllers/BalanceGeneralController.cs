using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.API.Controllers
{
    [ApiController]
    [Route("api/balance-general")]
    [Authorize]
    public class BalanceGeneralController : ControllerBase
    {
        private readonly IBalanceGeneralService _balanceGeneralService;

        public BalanceGeneralController(IBalanceGeneralService balanceGeneralService) => _balanceGeneralService = balanceGeneralService;

        [HttpGet]
        [RequierePermiso("Reportes", "Consultar")]
        public async Task<IActionResult> Generar([FromQuery] Guid clienteId, [FromQuery] DateOnly fechaHasta)
        {
            var dto = new BalanceGeneralFiltroDto
            {
                ClienteId = clienteId,
                FechaHasta = fechaHasta
            };

            return Ok(await _balanceGeneralService.Generar(dto));
        }
    }
}
