using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Constants;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Test;

public class ComprobanteServiceTests
{
    private readonly Mock<IComprobanteRepository> _comprobanteRepository;
    private readonly Mock<IClienteRepository> _clienteRepository;
    private readonly Mock<IAuditoriaService> _auditoriaService;
    private readonly Mock<IAsientoContableService> _asientoContableService;
    private readonly ComprobanteService _service;

    public ComprobanteServiceTests()
    {
        _comprobanteRepository = new Mock<IComprobanteRepository>();
        _clienteRepository = new Mock<IClienteRepository>();
        _auditoriaService = new Mock<IAuditoriaService>();
        _asientoContableService = new Mock<IAsientoContableService>();

        _service = new ComprobanteService(
            _comprobanteRepository.Object,
            _clienteRepository.Object,
            _auditoriaService.Object,
            _asientoContableService.Object);
    }

    [Fact]
    public async Task Crear_Exitoso_GuardaComprobante()
    {
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _clienteRepository
            .Setup(r => r.ObtenerPorId(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, RazonSocial = "Cliente Test" });

        _comprobanteRepository
            .Setup(r => r.ExisteDuplicado("A001", "123456789012", new DateOnly(2026, 6, 1), clienteId))
            .ReturnsAsync(false);

        var dto = new ComprobanteCrearDto
        {
            ClienteId = clienteId,
            Tipo = "Factura",
            Numero = "A001",
            RUT = "12.345.678/9012",
            Fecha = new DateOnly(2026, 6, 1),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m
        };

        var resultado = await _service.Crear(dto, usuarioId);

        Assert.Equal(clienteId, resultado.ClienteId);
        Assert.Equal("Factura", resultado.Tipo);
        Assert.Equal("123456789012", resultado.RUT);
        Assert.Equal(122m, resultado.ImporteTotal);

        _comprobanteRepository.Verify(r => r.Guardar(It.IsAny<Comprobante>()), Times.Once);
        _auditoriaService.Verify(a => a.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Crear,
            null,
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Crear_ConDuplicado_LanzaComprobanteDuplicadoException()
    {
        var clienteId = Guid.NewGuid();

        _clienteRepository
            .Setup(r => r.ObtenerPorId(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, RazonSocial = "Cliente Test" });

        _comprobanteRepository
            .Setup(r => r.ExisteDuplicado("A001", "123456789012", new DateOnly(2026, 6, 1), clienteId))
            .ReturnsAsync(true);

        var dto = new ComprobanteCrearDto
        {
            ClienteId = clienteId,
            Tipo = "Factura",
            Numero = "A001",
            RUT = "123456789012",
            Fecha = new DateOnly(2026, 6, 1),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m
        };

        await Assert.ThrowsAsync<ComprobanteDuplicadoException>(() => _service.Crear(dto, Guid.NewGuid()));

        _comprobanteRepository.Verify(r => r.Guardar(It.IsAny<Comprobante>()), Times.Never);
    }

    [Fact]
    public async Task Crear_RUTInvalido_LanzaRUTInvalidoException()
    {
        var clienteId = Guid.NewGuid();

        _clienteRepository
            .Setup(r => r.ObtenerPorId(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, RazonSocial = "Cliente Test" });

        var dto = new ComprobanteCrearDto
        {
            ClienteId = clienteId,
            Tipo = "Factura",
            Numero = "A001",
            RUT = "123",
            Fecha = new DateOnly(2026, 6, 1),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m
        };

        await Assert.ThrowsAsync<RUTInvalidoException>(() => _service.Crear(dto, Guid.NewGuid()));

        _comprobanteRepository.Verify(r => r.Guardar(It.IsAny<Comprobante>()), Times.Never);
    }

    [Fact]
    public async Task Modificar_ConAsiento_LanzaComprobanteConAsientoException()
    {
        var id = Guid.NewGuid();

        _comprobanteRepository
            .Setup(r => r.ObtenerPorId(id))
            .ReturnsAsync(new Comprobante
            {
                Id = id,
                ClienteId = Guid.NewGuid(),
                Tipo = TipoComprobante.Factura,
                Numero = "A001",
                RUT = "123456789012",
                Fecha = new DateOnly(2026, 6, 1),
                ImporteNeto = 100m,
                TasaIVA = 22m,
                ImporteIVA = 22m,
                ImporteTotal = 122m,
                Estado = EstadoComprobante.Activo,
                AsientoId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

        var dto = new ComprobanteModificarDto
        {
            Tipo = "Factura",
            Numero = "A001",
            RUT = "123456789012",
            Fecha = new DateOnly(2026, 6, 1),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m
        };

        await Assert.ThrowsAsync<ComprobanteConAsientoException>(() => _service.Modificar(id, dto, Guid.NewGuid()));

        _comprobanteRepository.Verify(r => r.Actualizar(It.IsAny<Comprobante>()), Times.Never);
    }

    [Fact]
    public async Task Anular_Exitoso_CambiaEstadoAAnulado()
    {
        var id = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var comprobanteActivo = new Comprobante
        {
            Id = id,
            ClienteId = Guid.NewGuid(),
            Tipo = TipoComprobante.Factura,
            Numero = "A001",
            RUT = "123456789012",
            Fecha = new DateOnly(2026, 6, 1),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m,
            Estado = EstadoComprobante.Activo,
            CreatedAt = DateTime.UtcNow
        };

        var comprobanteAnulado = new Comprobante
        {
            Id = id,
            ClienteId = comprobanteActivo.ClienteId,
            Tipo = comprobanteActivo.Tipo,
            Numero = comprobanteActivo.Numero,
            RUT = comprobanteActivo.RUT,
            Fecha = comprobanteActivo.Fecha,
            ImporteNeto = comprobanteActivo.ImporteNeto,
            TasaIVA = comprobanteActivo.TasaIVA,
            ImporteIVA = comprobanteActivo.ImporteIVA,
            ImporteTotal = comprobanteActivo.ImporteTotal,
            Estado = EstadoComprobante.Anulado,
            CreatedAt = comprobanteActivo.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.UtcNow
        };

        _comprobanteRepository
            .SetupSequence(r => r.ObtenerPorId(id))
            .ReturnsAsync(comprobanteActivo)
            .ReturnsAsync(comprobanteAnulado);

        await _service.Anular(id, usuarioId);

        _comprobanteRepository.Verify(r => r.Anular(id), Times.Once);
        _auditoriaService.Verify(a => a.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Anular,
            It.IsAny<object>(),
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Listar_ConFiltros_FiltraCorrectamente()
    {
        var clienteId = Guid.NewGuid();
        TipoComprobante? tipoRecibido = null;
        EstadoComprobante? estadoRecibido = null;
        string? rutRecibido = null;

        _comprobanteRepository
            .Setup(r => r.ObtenerPorFiltros(
                clienteId,
                It.IsAny<TipoComprobante?>(),
                It.IsAny<string?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<EstadoComprobante?>(),
                1,
                20))
            .Callback<Guid, TipoComprobante?, string?, DateOnly?, DateOnly?, EstadoComprobante?, int, int>((_, t, rut, _, _, e, _, _) =>
            {
                tipoRecibido = t;
                estadoRecibido = e;
                rutRecibido = rut;
            })
            .ReturnsAsync(new List<Comprobante>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ClienteId = clienteId,
                    Tipo = TipoComprobante.Factura,
                    Numero = "A001",
                    RUT = "123456789012",
                    Fecha = new DateOnly(2026, 6, 1),
                    ImporteNeto = 100m,
                    TasaIVA = 22m,
                    ImporteIVA = 22m,
                    ImporteTotal = 122m,
                    Estado = EstadoComprobante.Activo,
                    CreatedAt = DateTime.UtcNow
                }
            });

        var filtro = new FiltroComprobanteDto
        {
            ClienteId = clienteId,
            Tipo = "Factura",
            RUT = "12.345.678/9012",
            FechaDesde = new DateOnly(2026, 1, 1),
            FechaHasta = new DateOnly(2026, 12, 31),
            Estado = "Activo",
            Pagina = 1,
            CantidadPorPagina = 20
        };

        var resultado = await _service.Listar(filtro);

        Assert.Single(resultado);
        Assert.Equal(TipoComprobante.Factura, tipoRecibido);
        Assert.Equal(EstadoComprobante.Activo, estadoRecibido);
        Assert.Equal("123456789012", rutRecibido);
    }

    [Fact]
    public async Task GenerarAsiento_Exitoso_UsaMonedaNacionalYAsociaAsiento()
    {
        var comprobanteId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var ejercicioId = Guid.NewGuid();
        var cuentaDebeId = Guid.NewGuid();
        var cuentaHaberId = Guid.NewGuid();

        var comprobante = new Comprobante
        {
            Id = comprobanteId,
            ClienteId = clienteId,
            Tipo = TipoComprobante.Factura,
            Numero = "A001",
            RUT = "123456789012",
            Fecha = new DateOnly(2026, 6, 10),
            ImporteNeto = 100m,
            TasaIVA = 22m,
            ImporteIVA = 22m,
            ImporteTotal = 122m,
            Estado = EstadoComprobante.Activo,
            CreatedAt = DateTime.UtcNow
        };

        _comprobanteRepository
            .SetupSequence(r => r.ObtenerPorId(comprobanteId))
            .ReturnsAsync(comprobante)
            .ReturnsAsync(comprobante);

        CrearAsientoContableDto? asientoDto = null;
        _asientoContableService
            .Setup(s => s.Crear(It.IsAny<CrearAsientoContableDto>(), usuarioId))
            .Callback<CrearAsientoContableDto, Guid>((dto, _) => asientoDto = dto)
            .ReturnsAsync(new AsientoContableDto
            {
                Id = Guid.NewGuid(),
                Numero = 99,
                ClienteId = clienteId,
                EjercicioId = ejercicioId,
                Fecha = comprobante.Fecha,
                Estado = "Confirmado",
                Glosa = "Auto"
            });

        var dto = new GenerarAsientoDesdeComprobanteDto
        {
            EjercicioId = ejercicioId,
            CuentaDebeId = cuentaDebeId,
            CuentaHaberId = cuentaHaberId
        };

        var resultado = await _service.GenerarAsiento(comprobanteId, dto, usuarioId);

        Assert.NotNull(asientoDto);
        Assert.Equal(clienteId, asientoDto!.ClienteId);
        Assert.Equal(2, asientoDto.Lineas.Count);
        Assert.All(asientoDto.Lineas, l => Assert.Equal("UYU", l.Moneda));
        Assert.All(asientoDto.Lineas, l => Assert.Equal(1m, l.TipoCambio));
        Assert.Equal(resultado.Id, comprobante.AsientoId);

        _comprobanteRepository.Verify(r => r.Actualizar(It.IsAny<Comprobante>()), Times.Once);
    }
}
