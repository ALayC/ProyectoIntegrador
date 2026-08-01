using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class EstadoResultadosServiceTests
{
    private readonly Mock<ILineaAsientoRepository> _lineaAsientoRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly Mock<ICuentaContableRepository> _cuentaRepository;
    private readonly EstadoResultadosService _service;

    public EstadoResultadosServiceTests()
    {
        _lineaAsientoRepository = new Mock<ILineaAsientoRepository>();
        _planRepository = new Mock<IPlanDeCuentasRepository>();
        _cuentaRepository = new Mock<ICuentaContableRepository>();

        _service = new EstadoResultadosService(
            _lineaAsientoRepository.Object,
            _planRepository.Object,
            _cuentaRepository.Object,
            NullLogger<EstadoResultadosService>.Instance);
    }

    [Fact]
    public async Task Generar_CuandoFechaDesdeMayorQueFechaHasta_LanzaValidacionException()
    {
        await Assert.ThrowsAsync<ValidacionException>(() =>
            _service.Generar(new EstadoResultadosFiltroDto
            {
                ClienteId = Guid.NewGuid(),
                FechaDesde = new DateOnly(2026, 12, 31),
                FechaHasta = new DateOnly(2026, 1, 1)
            }));
    }

    [Fact]
    public async Task Generar_ConMovimientosValidos_CalculaResultadoNeto()
    {
        var clienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var ingresoId = Guid.NewGuid();
        var egresoId = Guid.NewGuid();

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync(new PlanDeCuentas { Id = planId, ClienteId = clienteId });

        _cuentaRepository
            .Setup(r => r.ObtenerTodasPorPlan(planId))
            .ReturnsAsync(new List<CuentaContable>
            {
                new() { Id = ingresoId, PlanCuentasId = planId, Codigo = "4", Nombre = "Ingresos", Tipo = "Ingreso", Naturaleza = "Acreedora" },
                new() { Id = egresoId, PlanCuentasId = planId, Codigo = "5", Nombre = "Egresos", Tipo = "Egreso", Naturaleza = "Deudora" }
            });

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaEstadoResultados(clienteId, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)))
            .ReturnsAsync(new List<LineaAsiento>
            {
                new() { CuentaContableId = ingresoId, Debe = 0m, Haber = 200m, TipoCambio = 1m },
                new() { CuentaContableId = egresoId, Debe = 80m, Haber = 0m, TipoCambio = 1m }
            });

        var resultado = await _service.Generar(new EstadoResultadosFiltroDto
        {
            ClienteId = clienteId,
            FechaDesde = new DateOnly(2026, 1, 1),
            FechaHasta = new DateOnly(2026, 12, 31)
        });

        Assert.Equal(200m, resultado.TotalIngresos);
        Assert.Equal(80m, resultado.TotalEgresos);
        Assert.Equal(120m, resultado.ResultadoNeto);
    }
}
