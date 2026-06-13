using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

/// <summary>
/// Tests de integración para AsientoContableService.
/// Valida las reglas de negocio críticas de asientos contables.
/// </summary>
public class AsientoContableServiceTests : IDisposable
{
    private readonly Mock<IAsientoContableRepository> _mockAsientoRepo;
    private readonly Mock<ICuentaContableRepository> _mockCuentaRepo;
    private readonly Mock<IEjercicioContableRepository> _mockEjercicioRepo;
    private readonly Mock<ISaldoCuentaRepository> _mockSaldoRepo;
    private readonly AppDbContext _context;
    private readonly AsientoContableService _service;

    public AsientoContableServiceTests()
    {
        // Configurar DbContext con InMemory Database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(warnings =>
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);

        // Mockear repositorios
        _mockAsientoRepo = new Mock<IAsientoContableRepository>();
        _mockCuentaRepo = new Mock<ICuentaContableRepository>();
        _mockEjercicioRepo = new Mock<IEjercicioContableRepository>();
        _mockSaldoRepo = new Mock<ISaldoCuentaRepository>();

        _service = new AsientoContableService(
            _mockAsientoRepo.Object,
            _mockCuentaRepo.Object,
            _mockEjercicioRepo.Object,
            _mockSaldoRepo.Object,
            _context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Test 1: Crear_ConDebeIgualHaber_GuardaExitosamente

    /// <summary>
    /// Test 1: Escenario exitoso (happy path)
    /// Verifica que un asiento balanceado se guarda correctamente
    /// con numeración secuencial.
    /// </summary>
    [Fact]
    public async Task Crear_ConDebeIgualHaber_GuardaExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ejercicioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var cuenta1Id = Guid.NewGuid();
        var cuenta2Id = Guid.NewGuid();

        var dto = new CrearAsientoContableDto
        {
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Glosa = "Asiento de prueba balanceado",
            Lineas = new List<LineaAsientoInputDto>
            {
                new()
                {
                    CuentaContableId = cuenta1Id,
                    Debe = 1000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                },
                new()
                {
                    CuentaContableId = cuenta2Id,
                    Debe = 0m,
                    Haber = 1000m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                }
            }
        };

        _mockEjercicioRepo
            .Setup(r => r.ObtenerPorId(ejercicioId))
            .ReturnsAsync(new EjercicioContable
            {
                Id = ejercicioId,
                ClienteId = clienteId,
                FechaInicio = new DateOnly(DateTime.Today.Year, 1, 1),
                FechaFin = new DateOnly(DateTime.Today.Year, 12, 31),
                Estado = "Abierto"
            });

        // Mock: Cuentas existen y son imputables
        _mockCuentaRepo
            .Setup(r => r.ObtenerPorId(cuenta1Id))
            .ReturnsAsync(new CuentaContable
            {
                Id = cuenta1Id,
                Codigo = "1.1.01",
                Nombre = "Caja",
                EsImputable = true,
                Estado = "Activa"
            });

        _mockCuentaRepo
            .Setup(r => r.ObtenerPorId(cuenta2Id))
            .ReturnsAsync(new CuentaContable
            {
                Id = cuenta2Id,
                Codigo = "4.1.01",
                Nombre = "Ingresos por servicios",
                EsImputable = true,
                Estado = "Activa"
            });

        // Mock: Último número de asiento
        _mockAsientoRepo
            .Setup(r => r.ObtenerUltimoNumero(clienteId, ejercicioId))
            .ReturnsAsync(5);

        // Mock: Guardar asiento
        _mockAsientoRepo
            .Setup(r => r.Guardar(It.IsAny<AsientoContable>()))
            .Returns(Task.CompletedTask);

        // Mock: Saldos no existen (se crearán nuevos)
        _mockSaldoRepo
            .Setup(r => r.ObtenerPorPeriodo(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync((SaldoCuenta?)null);

        _mockSaldoRepo
            .Setup(r => r.Guardar(It.IsAny<SaldoCuenta>()))
            .Returns(Task.CompletedTask);

        // Mock: ObtenerPorIdConLineas para retornar el asiento guardado
        var asientoGuardado = new AsientoContable
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            UsuarioId = usuarioId,
            Numero = 6,
            Fecha = dto.Fecha,
            Glosa = dto.Glosa,
            Estado = "Confirmado",
            LineasAsiento = new List<LineaAsiento>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta1Id,
                    Debe = 1000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 1000m,
                    CuentaContable = new CuentaContable
                    {
                        Id = cuenta1Id,
                        Codigo = "1.1.01",
                        Nombre = "Caja"
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta2Id,
                    Debe = 0m,
                    Haber = 1000m,
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 1000m,
                    CuentaContable = new CuentaContable
                    {
                        Id = cuenta2Id,
                        Codigo = "4.1.01",
                        Nombre = "Ingresos por servicios"
                    }
                }
            }
        };

        _mockAsientoRepo
            .Setup(r => r.ObtenerPorIdConLineas(It.IsAny<Guid>()))
            .ReturnsAsync(asientoGuardado);

        // Act
        var resultado = await _service.Crear(dto, usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Confirmado", resultado.Estado);
        Assert.Equal(6, resultado.Numero);
        Assert.Equal(2, resultado.Lineas.Count);
        Assert.Equal(1000m, resultado.TotalDebe);
        Assert.Equal(1000m, resultado.TotalHaber);

        // Verificar que se llamó a Guardar
        _mockAsientoRepo.Verify(
            r => r.Guardar(It.Is<AsientoContable>(a =>
                a.Numero == 6 &&
                a.Estado == "Confirmado" &&
                a.LineasAsiento.Count == 2)),
            Times.Once);

        // Verificar que se guardaron 2 saldos (uno por cada cuenta)
        _mockSaldoRepo.Verify(
            r => r.Guardar(It.IsAny<SaldoCuenta>()),
            Times.Exactly(2));
    }

    #endregion

    #region Test 2: Crear_ConDebeDistintoDeHaber_LanzaAsientoDesbalanceadoException

    /// <summary>
    /// Test 2: Validación de balance
    /// Verifica que se lanza excepción cuando Debe ≠ Haber.
    /// </summary>
    [Fact]
    public async Task Crear_ConDebeDistintoDeHaber_LanzaAsientoDesbalanceadoException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ejercicioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var cuenta1Id = Guid.NewGuid();
        var cuenta2Id = Guid.NewGuid();

        var dto = new CrearAsientoContableDto
        {
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Glosa = "Asiento desbalanceado (debe fallar)",
            Lineas = new List<LineaAsientoInputDto>
            {
                new()
                {
                    CuentaContableId = cuenta1Id,
                    Debe = 1000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                },
                new()
                {
                    CuentaContableId = cuenta2Id,
                    Debe = 0m,
                    Haber = 500m, // ❌ No balancea: 1000 ≠ 500
                    Moneda = "UYU",
                    TipoCambio = 1m
                }
            }
        };

        _mockEjercicioRepo
            .Setup(r => r.ObtenerPorId(ejercicioId))
            .ReturnsAsync(new EjercicioContable
            {
                Id = ejercicioId,
                ClienteId = clienteId,
                FechaInicio = new DateOnly(DateTime.Today.Year, 1, 1),
                FechaFin = new DateOnly(DateTime.Today.Year, 12, 31),
                Estado = "Abierto"
            });

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<AsientoDesbalanceadoException>(
            () => _service.Crear(dto, usuarioId));

        // Verificar que el mensaje contiene información sobre el desbalance
        Assert.Contains("desbalanceado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Debe", excepcion.Message);
        Assert.Contains("Haber", excepcion.Message);

        // Verificar que NO se llamó a Guardar
        _mockAsientoRepo.Verify(
            r => r.Guardar(It.IsAny<AsientoContable>()),
            Times.Never);
    }

    #endregion

    #region Test 3: Crear_ConCuentaNoImputable_LanzaCuentaNoImputableException

    /// <summary>
    /// Test 3: Validación de cuentas imputables
    /// Verifica que se lanza excepción al intentar usar una cuenta no imputable.
    /// </summary>
    [Fact]
    public async Task Crear_ConCuentaNoImputable_LanzaCuentaNoImputableException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ejercicioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var cuentaPadreId = Guid.NewGuid(); // Cuenta de nivel superior (no imputable)
        var cuentaHijaId = Guid.NewGuid();

        var dto = new CrearAsientoContableDto
        {
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Glosa = "Intento de usar cuenta no imputable",
            Lineas = new List<LineaAsientoInputDto>
            {
                new()
                {
                    CuentaContableId = cuentaPadreId, // ❌ Cuenta padre (no imputable)
                    Debe = 1000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                },
                new()
                {
                    CuentaContableId = cuentaHijaId,
                    Debe = 0m,
                    Haber = 1000m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                }
            }
        };

        _mockEjercicioRepo
            .Setup(r => r.ObtenerPorId(ejercicioId))
            .ReturnsAsync(new EjercicioContable
            {
                Id = ejercicioId,
                ClienteId = clienteId,
                FechaInicio = new DateOnly(DateTime.Today.Year, 1, 1),
                FechaFin = new DateOnly(DateTime.Today.Year, 12, 31),
                Estado = "Abierto"
            });

        // Mock: Primera cuenta NO es imputable
        _mockCuentaRepo
            .Setup(r => r.ObtenerPorId(cuentaPadreId))
            .ReturnsAsync(new CuentaContable
            {
                Id = cuentaPadreId,
                Codigo = "1",
                Nombre = "Activo",
                EsImputable = false, // ❌ No se puede usar en asientos
                Estado = "Activa"
            });

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<CuentaNoImputableException>(
            () => _service.Crear(dto, usuarioId));

        // Verificar que la excepción contiene el código de la cuenta
        Assert.Contains("1", excepcion.Message);

        // Verificar que NO se llamó a Guardar
        _mockAsientoRepo.Verify(
            r => r.Guardar(It.IsAny<AsientoContable>()),
            Times.Never);
    }

    #endregion

    #region Test 4: Revertir_AsientoConfirmado_CreaAsientoInverso

    /// <summary>
    /// Test 4: Reversión de asiento
    /// Verifica que se genera un asiento inverso (Debe ↔ Haber)
    /// y se marca el original como "Revertido".
    /// </summary>
    [Fact]
    public async Task Revertir_AsientoConfirmado_CreaAsientoInverso()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ejercicioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoOriginalId = Guid.NewGuid();
        var cuenta1Id = Guid.NewGuid();
        var cuenta2Id = Guid.NewGuid();

        var asientoOriginal = new AsientoContable
        {
            Id = asientoOriginalId,
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            UsuarioId = usuarioId,
            Numero = 10,
            Fecha = new DateOnly(2026, 5, 15),
            Glosa = "Asiento original a revertir",
            Estado = "Confirmado",
            LineasAsiento = new List<LineaAsiento>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta1Id,
                    Debe = 2000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 2000m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta2Id,
                    Debe = 0m,
                    Haber = 2000m,
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 2000m
                }
            }
        };

        // Mock: Obtener asiento original
        _mockAsientoRepo
            .Setup(r => r.ObtenerPorIdConLineas(asientoOriginalId))
            .ReturnsAsync(asientoOriginal);

        // Mock: Último número de asiento
        _mockAsientoRepo
            .Setup(r => r.ObtenerUltimoNumero(clienteId, ejercicioId))
            .ReturnsAsync(10);

        // Mock: Guardar asiento inverso
        _mockAsientoRepo
            .Setup(r => r.Guardar(It.IsAny<AsientoContable>()))
            .Returns(Task.CompletedTask);

        // Mock: Actualizar asiento original
        _mockAsientoRepo
            .Setup(r => r.Actualizar(It.IsAny<AsientoContable>()))
            .Returns(Task.CompletedTask);

        // Mock: Saldos
        _mockSaldoRepo
            .Setup(r => r.ObtenerPorPeriodo(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync((SaldoCuenta?)null);

        _mockSaldoRepo
            .Setup(r => r.Guardar(It.IsAny<SaldoCuenta>()))
            .Returns(Task.CompletedTask);

        // Mock: Asiento inverso guardado (para el return del método Revertir)
        var asientoInverso = new AsientoContable
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            EjercicioId = ejercicioId,
            UsuarioId = usuarioId,
            AsientoOrigenId = asientoOriginalId,
            Numero = 11,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Glosa = $"Reversión del asiento N° 10: Asiento original a revertir",
            Estado = "Confirmado",
            LineasAsiento = new List<LineaAsiento>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta1Id,
                    Debe = 0m,      // ✅ Invertido: era 2000 en Debe
                    Haber = 2000m,  // ✅ Invertido: era 0 en Haber
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 2000m,
                    CuentaContable = new CuentaContable
                    {
                        Id = cuenta1Id,
                        Codigo = "1.1.01",
                        Nombre = "Caja"
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuenta2Id,
                    Debe = 2000m,   // ✅ Invertido: era 0 en Debe
                    Haber = 0m,     // ✅ Invertido: era 2000 en Haber
                    Moneda = "UYU",
                    TipoCambio = 1m,
                    ImporteMonedaBase = 2000m,
                    CuentaContable = new CuentaContable
                    {
                        Id = cuenta2Id,
                        Codigo = "4.1.01",
                        Nombre = "Ingresos"
                    }
                }
            }
        };

        _mockAsientoRepo
            .Setup(r => r.ObtenerPorIdConLineas(It.Is<Guid>(id => id != asientoOriginalId)))
            .ReturnsAsync(asientoInverso);

        // Act
        var resultado = await _service.Revertir(asientoOriginalId, usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(11, resultado.Numero);
        Assert.Equal("Confirmado", resultado.Estado);
        Assert.Equal(asientoOriginalId, resultado.AsientoOrigenId);
        Assert.Contains("Reversión del asiento N° 10", resultado.Glosa);

        // Verificar que las líneas están invertidas
        Assert.Equal(2, resultado.Lineas.Count);
        Assert.Equal(0m, resultado.Lineas[0].Debe);    // Original: 2000 Debe → 0 Debe
        Assert.Equal(2000m, resultado.Lineas[0].Haber); // Original: 0 Haber → 2000 Haber
        Assert.Equal(2000m, resultado.Lineas[1].Debe);  // Original: 0 Debe → 2000 Debe
        Assert.Equal(0m, resultado.Lineas[1].Haber);    // Original: 2000 Haber → 0 Haber

        // Verificar que se guardó el asiento inverso
        _mockAsientoRepo.Verify(
            r => r.Guardar(It.Is<AsientoContable>(a =>
                a.AsientoOrigenId == asientoOriginalId &&
                a.Numero == 11)),
            Times.Once);

        // Verificar que se actualizó el original a "Revertido"
        _mockAsientoRepo.Verify(
            r => r.Actualizar(It.Is<AsientoContable>(a =>
                a.Id == asientoOriginalId &&
                a.Estado == "Revertido")),
            Times.Once);
    }

    #endregion

    #region Test 5: Revertir_AsientoYaRevertido_LanzaAsientoYaRevertidoException

    /// <summary>
    /// Test 5: Validación de reversión duplicada
    /// Verifica que no se puede revertir un asiento que ya fue revertido.
    /// </summary>
    [Fact]
    public async Task Revertir_AsientoYaRevertido_LanzaAsientoYaRevertidoException()
    {
        // Arrange
        var asientoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var asientoYaRevertido = new AsientoContable
        {
            Id = asientoId,
            ClienteId = Guid.NewGuid(),
            EjercicioId = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Numero = 20,
            Fecha = new DateOnly(2026, 5, 10),
            Glosa = "Asiento ya revertido",
            Estado = "Revertido", // ❌ Ya fue revertido
            LineasAsiento = new List<LineaAsiento>()
        };

        // Mock: Obtener asiento ya revertido
        _mockAsientoRepo
            .Setup(r => r.ObtenerPorIdConLineas(asientoId))
            .ReturnsAsync(asientoYaRevertido);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<AsientoYaRevertidoException>(
            () => _service.Revertir(asientoId, usuarioId));

        // Verificar que NO se guardó ningún asiento nuevo
        _mockAsientoRepo.Verify(
            r => r.Guardar(It.IsAny<AsientoContable>()),
            Times.Never);

        // Verificar que NO se actualizó el asiento
        _mockAsientoRepo.Verify(
            r => r.Actualizar(It.IsAny<AsientoContable>()),
            Times.Never);
    }

    #endregion

    #region Test 6 (Bonus): Crear_ConMenosDeDosLineas_LanzaValidacionException

    /// <summary>
    /// Test 6 (Bonus): Validación de líneas mínimas
    /// Verifica que un asiento debe tener al menos 2 líneas (principio de partida doble).
    /// </summary>
    [Fact]
    public async Task Crear_ConMenosDeDosLineas_LanzaValidacionException()
    {
        // Arrange
        var dto = new CrearAsientoContableDto
        {
            ClienteId = Guid.NewGuid(),
            EjercicioId = Guid.NewGuid(),
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Glosa = "Asiento inválido con 1 sola línea",
            Lineas = new List<LineaAsientoInputDto>
            {
                new()
                {
                    CuentaContableId = Guid.NewGuid(),
                    Debe = 1000m,
                    Haber = 0m,
                    Moneda = "UYU",
                    TipoCambio = 1m
                }
                // ❌ Solo 1 línea (debe tener al menos 2)
            }
        };

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<ValidacionException>(
            () => _service.Crear(dto, Guid.NewGuid()));

        Assert.Contains("al menos dos líneas", excepcion.Message);

        _mockAsientoRepo.Verify(
            r => r.Guardar(It.IsAny<AsientoContable>()),
            Times.Never);
    }

    #endregion
}