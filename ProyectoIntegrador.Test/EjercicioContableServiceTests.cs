using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Test;

public class EjercicioContableServiceTests : IDisposable
{
    private readonly Mock<IEjercicioContableRepository> _ejercicioRepository;
    private readonly Mock<IClienteRepository> _clienteRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly Mock<IAsientoContableRepository> _asientoRepository;
    private readonly Mock<ISaldoCuentaRepository> _saldoRepository;
    private readonly Mock<IAuditoriaService> _auditoriaService;
    private readonly AppDbContext _context;
    private readonly EjercicioContableService _service;

    public EjercicioContableServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"EjercicioTestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);

        _ejercicioRepository = new Mock<IEjercicioContableRepository>();
        _clienteRepository = new Mock<IClienteRepository>();
        _planRepository = new Mock<IPlanDeCuentasRepository>();
        _asientoRepository = new Mock<IAsientoContableRepository>();
        _saldoRepository = new Mock<ISaldoCuentaRepository>();
        _auditoriaService = new Mock<IAuditoriaService>();

        _service = new EjercicioContableService(
            _ejercicioRepository.Object,
            _clienteRepository.Object,
            _planRepository.Object,
            _asientoRepository.Object,
            _saldoRepository.Object,
            _context,
            _auditoriaService.Object,
            NullLogger<EjercicioContableService>.Instance);
    }

    [Fact]
    public async Task Crear_ConDatosValidos_GuardaEjercicioAbierto()
    {
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        EjercicioContable? guardado = null;

        _clienteRepository
            .Setup(r => r.ObtenerPorId(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, RazonSocial = "Cliente Test", Rut = "123456789012", TipoContribuyente = "ResponsableIVA", MonedaBase = "UYU", Estado = "Activo" });

        _ejercicioRepository
            .Setup(r => r.Guardar(It.IsAny<EjercicioContable>()))
            .Callback<EjercicioContable>(e => guardado = e)
            .Returns(Task.CompletedTask);

        _auditoriaService
            .Setup(a => a.Registrar(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var dto = new CrearEjercicioContableDto
        {
            ClienteId = clienteId,
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 12, 31),
            UsuarioId = usuarioId
        };

        var resultado = await _service.Crear(dto);

        Assert.NotNull(guardado);
        Assert.Equal("Abierto", guardado!.Estado);
        Assert.Equal(clienteId, resultado.ClienteId);
        Assert.Equal("Abierto", resultado.Estado);

        _ejercicioRepository.Verify(r => r.Guardar(It.IsAny<EjercicioContable>()), Times.Once);
    }

    [Fact]
    public async Task Actualizar_ConEjercicioCerrado_LanzaEjercicioCerradoException()
    {
        var ejercicioId = Guid.NewGuid();

        _ejercicioRepository
            .Setup(r => r.ObtenerPorId(ejercicioId))
            .ReturnsAsync(new EjercicioContable
            {
                Id = ejercicioId,
                ClienteId = Guid.NewGuid(),
                FechaInicio = new DateOnly(2026, 1, 1),
                FechaFin = new DateOnly(2026, 12, 31),
                Estado = "Cerrado"
            });

        await Assert.ThrowsAsync<EjercicioCerradoException>(() =>
            _service.Actualizar(ejercicioId, new ActualizarEjercicioContableDto
            {
                FechaInicio = new DateOnly(2026, 1, 1),
                FechaFin = new DateOnly(2026, 12, 31)
            }));

        _ejercicioRepository.Verify(r => r.Actualizar(It.IsAny<EjercicioContable>()), Times.Never);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
