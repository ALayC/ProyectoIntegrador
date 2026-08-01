using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Test;

public class CuentaContableServiceTests
{
    private readonly Mock<ICuentaContableRepository> _cuentaRepository;
    private readonly Mock<IPlanDeCuentasRepository> _planRepository;
    private readonly Mock<IAuditoriaService> _auditoriaService;
    private readonly CuentaContableService _service;

    public CuentaContableServiceTests()
    {
        _cuentaRepository = new Mock<ICuentaContableRepository>();
        _planRepository = new Mock<IPlanDeCuentasRepository>();
        _auditoriaService = new Mock<IAuditoriaService>();

        _service = new CuentaContableService(
            _cuentaRepository.Object,
            _planRepository.Object,
            _auditoriaService.Object,
            NullLogger<CuentaContableService>.Instance);
    }

    [Fact]
    public async Task Crear_ConCodigoDuplicado_LanzaCuentaDuplicadaException()
    {
        var planId = Guid.NewGuid();

        _planRepository
            .Setup(r => r.ObtenerPorId(planId))
            .ReturnsAsync(new PlanDeCuentas { Id = planId });

        _cuentaRepository
            .Setup(r => r.ExisteCodigo(planId, "1.1"))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<CuentaDuplicadaException>(() =>
            _service.Crear(planId, new CrearCuentaContableDto
            {
                Codigo = "1.1",
                Nombre = "Caja",
                Tipo = "Activo",
                Naturaleza = "Deudora",
                EsImputable = true
            }, Guid.NewGuid()));

        _cuentaRepository.Verify(r => r.Guardar(It.IsAny<CuentaContable>()), Times.Never);
    }

    [Fact]
    public async Task Activar_CuandoPadreEstaInactivo_LanzaValidacionException()
    {
        var cuentaId = Guid.NewGuid();
        var padreId = Guid.NewGuid();

        _cuentaRepository
            .Setup(r => r.ObtenerPorId(cuentaId))
            .ReturnsAsync(new CuentaContable
            {
                Id = cuentaId,
                CuentaPadreId = padreId,
                Codigo = "1.2.1",
                Nombre = "Hija",
                Estado = "Inactiva",
                EsSistema = false
            });

        _cuentaRepository
            .Setup(r => r.ObtenerPorId(padreId))
            .ReturnsAsync(new CuentaContable
            {
                Id = padreId,
                Codigo = "1.2",
                Nombre = "Padre",
                Estado = "Inactiva"
            });

        await Assert.ThrowsAsync<ValidacionException>(() => _service.Activar(cuentaId));

        _cuentaRepository.Verify(r => r.Actualizar(It.IsAny<CuentaContable>()), Times.Never);
    }

    [Fact]
    public async Task SiguienteCodigoHija_ConHijasExistentes_RetornaSiguienteConsecutivo()
    {
        var padreId = Guid.NewGuid();

        _cuentaRepository
            .Setup(r => r.ObtenerPorId(padreId))
            .ReturnsAsync(new CuentaContable { Id = padreId, Codigo = "1.2", Nombre = "Padre" });

        _cuentaRepository
            .Setup(r => r.ObtenerHijas(padreId))
            .ReturnsAsync(new List<CuentaContable>
            {
                new() { Id = Guid.NewGuid(), Codigo = "1.2.1" },
                new() { Id = Guid.NewGuid(), Codigo = "1.2.3" }
            });

        var resultado = await _service.SiguienteCodigoHija(padreId);

        Assert.Equal("1.2.4", resultado);
    }
}
