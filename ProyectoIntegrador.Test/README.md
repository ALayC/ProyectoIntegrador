# ProyectoIntegrador.Test

## Qué es esta capa

Contiene los tests unitarios del sistema. Se testea exclusivamente la capa de servicios (ProyectoIntegrador.Service), que es donde reside la lógica de negocio crítica. No se testean controllers ni repositorios.

## Qué contiene

- Una clase de test por cada servicio (AsientoServiceTests.cs, ClienteServiceTests.cs, etc.)
- Mocks de repositorios usando la librería Moq

## Cobertura mínima

2 tests por servicio:
1. Un caso exitoso (happy path)
2. Un caso de error o validación de regla de negocio

## Paquetes NuGet necesarios

```
xUnit
Moq
Microsoft.NET.Test.Sdk
FluentAssertions (opcional, para assertions más legibles)
```

## Nomenclatura de tests

Patrón: `MetodoQueTesteas_Escenario_ResultadoEsperado`

```csharp
CrearAsiento_ConDebeIgualAHaber_GuardaExitosamente()
CrearAsiento_ConDebeDistintoDeHaber_LanzaAsientoNoBalanceadoException()
CerrarEjercicio_ConEjercicioYaCerrado_LanzaEjercicioCerradoException()
```

## Cómo crear un test

```csharp
using Moq;
using Xunit;

public class AsientoServiceTests
{
    private readonly Mock<IAsientoRepository> _asientoRepoMock;
    private readonly Mock<IEjercicioRepository> _ejercicioRepoMock;
    private readonly Mock<IAuditoriaRepository> _auditoriaRepoMock;
    private readonly AsientoService _service;

    // El constructor prepara los mocks y el servicio para cada test
    public AsientoServiceTests()
    {
        _asientoRepoMock = new Mock<IAsientoRepository>();
        _ejercicioRepoMock = new Mock<IEjercicioRepository>();
        _auditoriaRepoMock = new Mock<IAuditoriaRepository>();

        _service = new AsientoService(
            _asientoRepoMock.Object,
            _ejercicioRepoMock.Object,
            _auditoriaRepoMock.Object
        );
    }

    [Fact]
    public async Task CrearAsiento_ConDebeIgualAHaber_GuardaExitosamente()
    {
        // Arrange: preparar los datos de entrada
        var dto = new AsientoDto
        {
            Fecha = DateTime.Today,
            Glosa = "Test",
            Lineas = new List<LineaAsientoDto>
            {
                new() { CuentaContableId = Guid.NewGuid(), Debe = 1000, Haber = 0 },
                new() { CuentaContableId = Guid.NewGuid(), Debe = 0, Haber = 1000 },
            }
        };

        // Configurar mocks para que devuelvan lo esperado
        _ejercicioRepoMock
            .Setup(r => r.ObtenerAbierto(It.IsAny<Guid>()))
            .ReturnsAsync(new EjercicioContable { Estado = "Abierto" });

        // Act: ejecutar el método que queremos testear
        var resultado = await _service.Crear(dto);

        // Assert: verificar que el resultado es correcto
        Assert.NotNull(resultado);
        _asientoRepoMock.Verify(r => r.Guardar(It.IsAny<AsientoContable>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsiento_ConDebeDistintoDeHaber_LanzaExcepcion()
    {
        // Arrange
        var dto = new AsientoDto
        {
            Lineas = new List<LineaAsientoDto>
            {
                new() { Debe = 1000, Haber = 0 },
                new() { Debe = 0, Haber = 500 }, // No balancea
            }
        };

        _ejercicioRepoMock
            .Setup(r => r.ObtenerAbierto(It.IsAny<Guid>()))
            .ReturnsAsync(new EjercicioContable { Estado = "Abierto" });

        // Act & Assert: verificar que lanza la excepción esperada
        await Assert.ThrowsAsync<AsientoNoBalanceadoException>(
            () => _service.Crear(dto)
        );
    }
}
```

## Cómo ejecutar los tests

Desde Visual Studio: Test → Run All Tests (Ctrl+R, A)

Desde la terminal:
```bash
dotnet test ProyectoIntegrador.Test
```

## Referencia a otros proyectos

- ✅ ProyectoIntegrador.Service
- ✅ ProyectoIntegrador.Data
