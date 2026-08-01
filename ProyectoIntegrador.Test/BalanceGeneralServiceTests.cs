using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class BalanceGeneralServiceTests
{
    private readonly Mock<ILineaAsientoRepository> _lineaAsientoRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly Mock<ICuentaContableRepository> _cuentaRepository;
    private readonly BalanceGeneralService _service;

    public BalanceGeneralServiceTests()
    {
        _lineaAsientoRepository = new Mock<ILineaAsientoRepository>();
        _planRepository = new Mock<IPlanDeCuentasRepository>();
        _cuentaRepository = new Mock<ICuentaContableRepository>();

        _service = new BalanceGeneralService(
            _lineaAsientoRepository.Object,
            _planRepository.Object,
            _cuentaRepository.Object,
            NullLogger<BalanceGeneralService>.Instance);
    }

    [Fact]
    public async Task Generar_CuandoNoExistePlan_LanzaEntidadNoEncontradaException()
    {
        var clienteId = Guid.NewGuid();

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync((PlanDeCuentas?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() =>
            _service.Generar(new BalanceGeneralFiltroDto
            {
                ClienteId = clienteId,
                FechaHasta = new DateOnly(2026, 12, 31)
            }));
    }

    [Fact]
    public async Task Generar_ConMovimientosValidos_CalculaTotalesYBalancea()
    {
        var clienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var activoId = Guid.NewGuid();
        var pasivoId = Guid.NewGuid();
        var patrimonioId = Guid.NewGuid();

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync(new PlanDeCuentas { Id = planId, ClienteId = clienteId });

        _cuentaRepository
            .Setup(r => r.ObtenerTodasPorPlan(planId))
            .ReturnsAsync(new List<CuentaContable>
            {
                new() { Id = activoId, PlanCuentasId = planId, Codigo = "1", Nombre = "Activo", Tipo = "Activo", Naturaleza = "Deudora", Estado = "Activa" },
                new() { Id = pasivoId, PlanCuentasId = planId, Codigo = "2", Nombre = "Pasivo", Tipo = "Pasivo", Naturaleza = "Acreedora", Estado = "Activa" },
                new() { Id = patrimonioId, PlanCuentasId = planId, Codigo = "3", Nombre = "Patrimonio", Tipo = "Patrimonio", Naturaleza = "Acreedora", Estado = "Activa" }
            });

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaBalanceGeneral(clienteId, new DateOnly(2026, 12, 31)))
            .ReturnsAsync(new List<LineaAsiento>
            {
                new() { CuentaContableId = activoId, Debe = 100m, Haber = 0m, TipoCambio = 1m },
                new() { CuentaContableId = pasivoId, Debe = 0m, Haber = 40m, TipoCambio = 1m },
                new() { CuentaContableId = patrimonioId, Debe = 0m, Haber = 60m, TipoCambio = 1m }
            });

        var resultado = await _service.Generar(new BalanceGeneralFiltroDto
        {
            ClienteId = clienteId,
            FechaHasta = new DateOnly(2026, 12, 31)
        });

        Assert.Equal(100m, resultado.TotalActivo);
        Assert.Equal(40m, resultado.TotalPasivo);
        Assert.Equal(60m, resultado.TotalPatrimonio);
        Assert.Equal(100m, resultado.TotalPasivoPatrimonio);
        Assert.True(resultado.Balancea);
    }
}
