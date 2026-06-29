using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class LiquidacionIvaServiceTests
{
    private readonly Mock<ILineaAsientoRepository> _lineaAsientoRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly LiquidacionIvaService _service;

    // IDs reutilizables para cuentas IVA
    private readonly Guid _idIvaDebito  = Guid.NewGuid();
    private readonly Guid _idIvaCredito = Guid.NewGuid();

    public LiquidacionIvaServiceTests()
    {
        _lineaAsientoRepository = new Mock<ILineaAsientoRepository>();
        _planRepository         = new Mock<IPlanDeCuentasRepository>();

        _service = new LiquidacionIvaService(
            _lineaAsientoRepository.Object,
            _planRepository.Object,
            NullLogger<LiquidacionIvaService>.Instance);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    private void ConfigurarPlanExistente(Guid clienteId)
    {
        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync(new PlanDeCuentas { Id = Guid.NewGuid(), ClienteId = clienteId });
    }

    private void ConfigurarLineas(Guid clienteId, int mes, int anio, List<LineaAsiento> lineas)
    {
        var desde = new DateOnly(anio, mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaLiquidacionIva(clienteId, desde, hasta))
            .ReturnsAsync(lineas);
    }

    private static LineaAsiento LineaDebito(Guid cuentaId, decimal debe, decimal haber) =>
        new()
        {
            CuentaContableId = cuentaId,
            Debe  = debe,
            Haber = haber,
            CuentaContable = new CuentaContable
            {
                Id        = cuentaId,
                Nombre    = "IVA Débito Fiscal",
                Tipo      = "Pasivo",
                Naturaleza = "Acreedora"
            }
        };

    private static LineaAsiento LineaCredito(Guid cuentaId, decimal debe, decimal haber) =>
        new()
        {
            CuentaContableId = cuentaId,
            Debe  = debe,
            Haber = haber,
            CuentaContable = new CuentaContable
            {
                Id        = cuentaId,
                Nombre    = "IVA Crédito Fiscal",
                Tipo      = "Activo",
                Naturaleza = "Deudora"
            }
        };

    // ─────────────────────────────────────────────────────────────
    // Tests de caso feliz
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Calcular_ConLos4AsientosDeEjemplo_RetornaAPagarMilCien()
    {
        // Los 4 asientos descriptos en la documentación de test:
        //   Asiento 1: venta → IVA Débito Fiscal Haber 2.200
        //   Asiento 2: venta → IVA Débito Fiscal Haber 1.100
        //   Asiento 3: compra → IVA Crédito Fiscal Debe 1.760
        //   Asiento 4: compra → IVA Crédito Fiscal Debe 440
        // Resultado esperado: Ventas 3.300 − Compras 2.200 = 1.100 APagar

        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaDebito (_idIvaDebito,     0m,  2200m),  // Asiento 1
            LineaDebito (_idIvaDebito,     0m,  1100m),  // Asiento 2
            LineaCredito(_idIvaCredito, 1760m,     0m),  // Asiento 3
            LineaCredito(_idIvaCredito,  440m,     0m),  // Asiento 4
        };
        ConfigurarLineas(clienteId, 6, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 6,
            Anio = 2025
        });

        Assert.Equal(6,        resultado.Mes);
        Assert.Equal(2025,     resultado.Anio);
        Assert.Equal(3300m,    resultado.TotalIvaVentas);
        Assert.Equal(2200m,    resultado.TotalIvaCompras);
        Assert.Equal(1100m,    resultado.SaldoNeto);
        Assert.Equal("APagar", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_CuandoComprasSuperanVentas_RetornaSaldoAFavor()
    {
        // IVA Ventas: 500 | IVA Compras: 2.000 → neto = −1.500 → AFavor
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaDebito (_idIvaDebito,     0m,  500m),
            LineaCredito(_idIvaCredito, 2000m,    0m),
        };
        ConfigurarLineas(clienteId, 3, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 3,
            Anio = 2025
        });

        Assert.Equal(500m,    resultado.TotalIvaVentas);
        Assert.Equal(2000m,   resultado.TotalIvaCompras);
        Assert.Equal(1500m,   resultado.SaldoNeto);
        Assert.Equal("AFavor", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_SinMovimientosDeIva_RetornaCerosYTipoAFavor()
    {
        // Sin asientos IVA en el período → todo cero → TipoSaldo = "AFavor" (neto 0, no es APagar)
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);
        ConfigurarLineas(clienteId, 1, 2025, new List<LineaAsiento>());

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 1,
            Anio = 2025
        });

        Assert.Equal(0m,      resultado.TotalIvaVentas);
        Assert.Equal(0m,      resultado.TotalIvaCompras);
        Assert.Equal(0m,      resultado.SaldoNeto);
        Assert.Equal("AFavor", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_SoloIvaVentas_SinCompras_RetornaAPagar()
    {
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaDebito(_idIvaDebito, 0m, 1500m),
        };
        ConfigurarLineas(clienteId, 2, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 2,
            Anio = 2025
        });

        Assert.Equal(1500m,    resultado.TotalIvaVentas);
        Assert.Equal(0m,       resultado.TotalIvaCompras);
        Assert.Equal(1500m,    resultado.SaldoNeto);
        Assert.Equal("APagar", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_SoloIvaCompras_SinVentas_RetornaAFavor()
    {
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaCredito(_idIvaCredito, 800m, 0m),
        };
        ConfigurarLineas(clienteId, 4, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 4,
            Anio = 2025
        });

        Assert.Equal(0m,      resultado.TotalIvaVentas);
        Assert.Equal(800m,    resultado.TotalIvaCompras);
        Assert.Equal(800m,    resultado.SaldoNeto);
        Assert.Equal("AFavor", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_CuandoVentasIgualesACompras_SaldoNuloCategoriaAFavor()
    {
        // Neto exactamente 0 → SaldoNeto = 0, TipoSaldo = "AFavor"
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaDebito (_idIvaDebito,     0m, 1000m),
            LineaCredito(_idIvaCredito, 1000m,    0m),
        };
        ConfigurarLineas(clienteId, 5, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 5,
            Anio = 2025
        });

        Assert.Equal(1000m,   resultado.TotalIvaVentas);
        Assert.Equal(1000m,   resultado.TotalIvaCompras);
        Assert.Equal(0m,      resultado.SaldoNeto);
        Assert.Equal("AFavor", resultado.TipoSaldo);
    }

    [Fact]
    public async Task Calcular_LineasConDebeYHaberEnMismaCuenta_AplicaNeteoCorrectamente()
    {
        // IVA Débito Fiscal puede tener reversiones (Debe > 0): saldo neto = Haber - Debe
        // Línea 1: Haber 2.000 | Línea 2 (reversión): Debe 500 → neto ventas = 1.500
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);

        var lineas = new List<LineaAsiento>
        {
            LineaDebito(_idIvaDebito,    0m, 2000m),
            LineaDebito(_idIvaDebito,  500m,    0m),  // reversión parcial
            LineaCredito(_idIvaCredito, 600m,   0m),
        };
        ConfigurarLineas(clienteId, 7, 2025, lineas);

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 7,
            Anio = 2025
        });

        // Ventas: (0-2000) + (500-0) = -2000 + 500 = ... hmm, la lógica es Haber - Debe por línea
        // Línea 1: 2000 - 0 = 2000 | Línea 2: 0 - 500 = -500 → totalIvaVentas = 1500
        // Compras: 600 - 0 = 600
        // Neto: 1500 - 600 = 900 → APagar
        Assert.Equal(1500m,    resultado.TotalIvaVentas);
        Assert.Equal(600m,     resultado.TotalIvaCompras);
        Assert.Equal(900m,     resultado.SaldoNeto);
        Assert.Equal("APagar", resultado.TipoSaldo);
    }

    // ─────────────────────────────────────────────────────────────
    // Tests de validaciones y casos de error
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task Calcular_CuandoMesInvalido_LanzaValidacionException(int mes)
    {
        var ex = await Assert.ThrowsAsync<ValidacionException>(() =>
            _service.Calcular(new LiquidacionIvaFiltroDto
            {
                ClienteId = Guid.NewGuid(),
                Mes  = mes,
                Anio = 2025
            }));

        Assert.Contains("mes", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    [InlineData(0)]
    public async Task Calcular_CuandoAnioInvalido_LanzaValidacionException(int anio)
    {
        var ex = await Assert.ThrowsAsync<ValidacionException>(() =>
            _service.Calcular(new LiquidacionIvaFiltroDto
            {
                ClienteId = Guid.NewGuid(),
                Mes  = 6,
                Anio = anio
            }));

        Assert.Contains("año", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Calcular_CuandoClienteSinPlan_LanzaEntidadNoEncontradaException()
    {
        var clienteId = Guid.NewGuid();

        _planRepository
            .Setup(r => r.ObtenerPorClienteId(clienteId))
            .ReturnsAsync((PlanDeCuentas?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() =>
            _service.Calcular(new LiquidacionIvaFiltroDto
            {
                ClienteId = clienteId,
                Mes  = 6,
                Anio = 2025
            }));
    }

    [Fact]
    public async Task Calcular_CuandoMesEsValido_NoLlamaAlRepositorioDeFechasInCorrectas()
    {
        // Verifica que el rango de fechas pasado al repositorio sea correcto para junio 2025
        var clienteId = Guid.NewGuid();
        var desdeEsperada = new DateOnly(2025, 6, 1);
        var hastaEsperada = new DateOnly(2025, 6, 30);

        ConfigurarPlanExistente(clienteId);

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada))
            .ReturnsAsync(new List<LineaAsiento>());

        await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 6,
            Anio = 2025
        });

        _lineaAsientoRepository.Verify(
            r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada),
            Times.Once);
    }

    [Fact]
    public async Task Calcular_CuandoMesEsEnero_FechaHastaEs31Enero()
    {
        // Verifica el cálculo correcto del último día para un mes de 31 días
        var clienteId = Guid.NewGuid();
        var desdeEsperada = new DateOnly(2025, 1, 1);
        var hastaEsperada = new DateOnly(2025, 1, 31);

        ConfigurarPlanExistente(clienteId);

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada))
            .ReturnsAsync(new List<LineaAsiento>());

        await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 1,
            Anio = 2025
        });

        _lineaAsientoRepository.Verify(
            r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada),
            Times.Once);
    }

    [Fact]
    public async Task Calcular_CuandoMesEsFebrero_FechaHastaEs28OFebreroSegunAnio()
    {
        // Verifica que febrero de un año no bisiesto termine el 28
        var clienteId = Guid.NewGuid();
        var desdeEsperada = new DateOnly(2025, 2, 1);
        var hastaEsperada = new DateOnly(2025, 2, 28);

        ConfigurarPlanExistente(clienteId);

        _lineaAsientoRepository
            .Setup(r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada))
            .ReturnsAsync(new List<LineaAsiento>());

        await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 2,
            Anio = 2025
        });

        _lineaAsientoRepository.Verify(
            r => r.ObtenerParaLiquidacionIva(clienteId, desdeEsperada, hastaEsperada),
            Times.Once);
    }

    [Fact]
    public async Task Calcular_DevuelveElMesYAnioDelFiltro()
    {
        // El DTO de respuesta debe reflejar exactamente el período solicitado
        var clienteId = Guid.NewGuid();
        ConfigurarPlanExistente(clienteId);
        ConfigurarLineas(clienteId, 11, 2024, new List<LineaAsiento>());

        var resultado = await _service.Calcular(new LiquidacionIvaFiltroDto
        {
            ClienteId = clienteId,
            Mes  = 11,
            Anio = 2024
        });

        Assert.Equal(11,   resultado.Mes);
        Assert.Equal(2024, resultado.Anio);
    }
}
