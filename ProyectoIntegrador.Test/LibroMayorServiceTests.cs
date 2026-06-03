using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class LibroMayorServiceTests
{
    private readonly Mock<IAsientoContableRepository> _asientoRepository;
    private readonly Mock<ICuentaContableRepository> _cuentaRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly Mock<IEjercicioContableRepository> _ejercicioRepository;
    private readonly LibroMayorService _service;

    public LibroMayorServiceTests()
    {
        _asientoRepository = new Mock<IAsientoContableRepository>();
        _cuentaRepository = new Mock<ICuentaContableRepository>();
        _planRepository = new Mock<IPlanDeCuentasRepository>();
        _ejercicioRepository = new Mock<IEjercicioContableRepository>();

        _service = new LibroMayorService(
            _asientoRepository.Object,
            _cuentaRepository.Object,
            _planRepository.Object,
            _ejercicioRepository.Object);
    }

    [Fact]
    public async Task Obtener_ConNaturalezaDeudora_CalculaSaldosCorrectos()
    {
        var clienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var cuentaId = Guid.NewGuid();
        var fechaDesde = new DateOnly(2026, 1, 1);
        var fechaHasta = new DateOnly(2026, 12, 31);

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync(new PlanDeCuentas { Id = planId, ClienteId = clienteId });

        _cuentaRepository
            .Setup(r => r.ObtenerTodasPorPlan(planId))
            .ReturnsAsync(new List<CuentaContable>
            {
                new()
                {
                    Id = cuentaId,
                    Codigo = "1.1.1",
                    Nombre = "Caja",
                    Tipo = "Activo",
                    Naturaleza = "Deudora"
                }
            });

        var inicial = new List<LineaAsiento>
        {
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 100m,
                Haber = 0m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2025, 12, 31),
                    Numero = 1,
                    Glosa = "Saldo inicial",
                    Estado = "Confirmado"
                }
            }
        };

        var periodo = new List<LineaAsiento>
        {
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 50m,
                Haber = 0m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2026, 2, 1),
                    Numero = 2,
                    Glosa = "Movimiento 1",
                    Estado = "Confirmado"
                }
            },
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 0m,
                Haber = 20m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2026, 3, 1),
                    Numero = 3,
                    Glosa = "Movimiento 2",
                    Estado = "Confirmado"
                }
            }
        };

        _asientoRepository
            .Setup(r => r.ObtenerMovimientosMayor(clienteId, It.IsAny<IEnumerable<Guid>>(), fechaDesde, fechaHasta, null))
            .ReturnsAsync(periodo);

        _asientoRepository
            .Setup(r => r.ObtenerMovimientosMayor(clienteId, It.IsAny<IEnumerable<Guid>>(), null, fechaDesde.AddDays(-1), null))
            .ReturnsAsync(inicial);

        var filtro = new LibroMayorFiltroDto
        {
            ClienteId = clienteId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };

        var resultado = await _service.Obtener(filtro);

        var cuenta = Assert.Single(resultado.Cuentas);
        Assert.Equal(100m, cuenta.SaldoInicial);
        Assert.Equal(50m, cuenta.Debitos);
        Assert.Equal(20m, cuenta.Creditos);
        Assert.Equal(130m, cuenta.SaldoFinal);

        Assert.Equal(2, cuenta.Movimientos.Count);
        Assert.Equal(150m, cuenta.Movimientos[0].SaldoAcumulado);
        Assert.Equal(130m, cuenta.Movimientos[1].SaldoAcumulado);
    }

    [Fact]
    public async Task Obtener_ConNaturalezaAcreedora_CalculaSaldosCorrectos()
    {
        var clienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var cuentaId = Guid.NewGuid();
        var fechaDesde = new DateOnly(2026, 1, 1);
        var fechaHasta = new DateOnly(2026, 12, 31);

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync(new PlanDeCuentas { Id = planId, ClienteId = clienteId });

        _cuentaRepository
            .Setup(r => r.ObtenerTodasPorPlan(planId))
            .ReturnsAsync(new List<CuentaContable>
            {
                new()
                {
                    Id = cuentaId,
                    Codigo = "2.1.1",
                    Nombre = "Proveedores",
                    Tipo = "Pasivo",
                    Naturaleza = "Acreedora"
                }
            });

        var inicial = new List<LineaAsiento>
        {
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 0m,
                Haber = 200m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2025, 12, 31),
                    Numero = 1,
                    Glosa = "Saldo inicial",
                    Estado = "Confirmado"
                }
            }
        };

        var periodo = new List<LineaAsiento>
        {
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 20m,
                Haber = 0m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2026, 2, 1),
                    Numero = 2,
                    Glosa = "Pago parcial",
                    Estado = "Confirmado"
                }
            },
            new()
            {
                CuentaContableId = cuentaId,
                Debe = 0m,
                Haber = 80m,
                Moneda = "UYU",
                TipoCambio = 1m,
                Asiento = new AsientoContable
                {
                    ClienteId = clienteId,
                    Fecha = new DateOnly(2026, 3, 1),
                    Numero = 3,
                    Glosa = "Nuevo gasto",
                    Estado = "Confirmado"
                }
            }
        };

        _asientoRepository
            .Setup(r => r.ObtenerMovimientosMayor(clienteId, It.IsAny<IEnumerable<Guid>>(), fechaDesde, fechaHasta, null))
            .ReturnsAsync(periodo);

        _asientoRepository
            .Setup(r => r.ObtenerMovimientosMayor(clienteId, It.IsAny<IEnumerable<Guid>>(), null, fechaDesde.AddDays(-1), null))
            .ReturnsAsync(inicial);

        var filtro = new LibroMayorFiltroDto
        {
            ClienteId = clienteId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };

        var resultado = await _service.Obtener(filtro);

        var cuenta = Assert.Single(resultado.Cuentas);
        Assert.Equal(200m, cuenta.SaldoInicial);
        Assert.Equal(20m, cuenta.Debitos);
        Assert.Equal(80m, cuenta.Creditos);
        Assert.Equal(260m, cuenta.SaldoFinal);

        Assert.Equal(2, cuenta.Movimientos.Count);
        Assert.Equal(180m, cuenta.Movimientos[0].SaldoAcumulado);
        Assert.Equal(260m, cuenta.Movimientos[1].SaldoAcumulado);
    }
}
