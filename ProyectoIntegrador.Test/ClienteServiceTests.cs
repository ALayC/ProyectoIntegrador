using Moq;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _mockClienteRepo;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
    private readonly Mock<IPlanDeCuentasRepository> _mockPlanRepo;
    private readonly Mock<IAuditoriaRepository> _mockAuditoriaRepo;
    private readonly ClienteService _clienteService;

    public ClienteServiceTests()
    {
        _mockClienteRepo = new Mock<IClienteRepository>();
        _mockUsuarioRepo = new Mock<IUsuarioRepository>();
        _mockPlanRepo = new Mock<IPlanDeCuentasRepository>();
        _mockAuditoriaRepo = new Mock<IAuditoriaRepository>();

        _clienteService = new ClienteService(
       _mockClienteRepo.Object,
   _mockUsuarioRepo.Object,
       _mockPlanRepo.Object,
       _mockAuditoriaRepo.Object);
    }

    [Fact]
    public async Task Crear_ConDatosValidos_GuardaClienteYPlanDeCuentas()
    {
        // Arrange
        var contadorId = Guid.NewGuid();
        var clienteDto = new ClienteDto
        {
            Rut = "219999870015",
            RazonSocial = "Empresa Test S.A.",
            NombreFantasia = "Test",
            Email = "empresa@test.com",
            Telefono = "099123456",
            TipoContribuyente = "ResponsableIVA",
            MonedaBase = "UYU"
        };

        var rolContador = new Rol
        {
            Id = SeedData.RolContadorId,
            Nombre = "Contador",
            EsPredefinido = true
        };

        var contador = new Usuario
        {
            Id = contadorId,
            Email = "contador@test.com",
            NombreCompleto = "Contador Test",
            RolId = rolContador.Id,
            Rol = rolContador,
            ProveedorAuth = "Local",
            Estado = "Activo",
            CreatedAt = DateTime.UtcNow
        };

        _mockClienteRepo
            .Setup(r => r.ExisteRut(clienteDto.Rut))
   .ReturnsAsync(false);

        _mockUsuarioRepo
         .Setup(r => r.ObtenerPorId(contadorId))
            .ReturnsAsync(contador);

        _mockClienteRepo
         .Setup(r => r.Guardar(It.IsAny<Cliente>()))
   .Returns(Task.CompletedTask);

        _mockPlanRepo
          .Setup(r => r.Guardar(It.IsAny<PlanDeCuentas>()))
        .Returns(Task.CompletedTask);

        _mockAuditoriaRepo
            .Setup(r => r.Guardar(It.IsAny<Auditoria>()))
       .Returns(Task.CompletedTask);

        // Act
        var resultado = await _clienteService.Crear(clienteDto, contadorId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(clienteDto.Rut, resultado.Rut);
        Assert.Equal(clienteDto.RazonSocial, resultado.RazonSocial);
        Assert.Equal(clienteDto.NombreFantasia, resultado.NombreFantasia);
        Assert.Equal(clienteDto.Email, resultado.Email);
        Assert.Equal(clienteDto.TipoContribuyente, resultado.TipoContribuyente);
        Assert.Equal(clienteDto.MonedaBase, resultado.MonedaBase);
        Assert.Equal("Activo", resultado.Estado);
        Assert.Equal(contadorId, resultado.ContadorId);

        // Verificar que se guardó el cliente
        _mockClienteRepo.Verify(r => r.Guardar(It.Is<Cliente>(c =>
                 c.Rut == clienteDto.Rut &&
                 c.RazonSocial == clienteDto.RazonSocial &&
           c.ContadorId == contadorId &&
             c.Estado == "Activo"
         )), Times.Once);

        // Verificar que se creó el PlanDeCuentas
        _mockPlanRepo.Verify(r => r.Guardar(It.Is<PlanDeCuentas>(p =>
           p.ClienteId == resultado.Id
       )), Times.Once);

        // Verificar que se registró auditoría
        _mockAuditoriaRepo.Verify(r => r.Guardar(It.Is<Auditoria>(a =>
            a.Entidad == "Cliente" &&
           a.Accion == "Crear" &&
       a.UsuarioId == contadorId &&
                 a.DatosAnteriores == null &&
                 a.DatosNuevos != null
             )), Times.Once);
    }

    [Fact]
    public async Task Crear_ConRutDuplicado_LanzaDuplicadoException()
    {
        // Arrange
        var contadorId = Guid.NewGuid();
        var clienteDto = new ClienteDto
        {
            Rut = "219999870015",
            RazonSocial = "Empresa Duplicada S.A.",
            Email = "duplicada@test.com",
            Telefono = "099654321",
            TipoContribuyente = "Monotributo",
            MonedaBase = "UYU"
        };

        _mockClienteRepo
            .Setup(r => r.ExisteRut(clienteDto.Rut))
 .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicadoException>(
   () => _clienteService.Crear(clienteDto, contadorId));

        Assert.Contains("219999870015", exception.Message);

        // Verificar que NO se intentó guardar
        _mockClienteRepo.Verify(r => r.Guardar(It.IsAny<Cliente>()), Times.Never);
        _mockPlanRepo.Verify(r => r.Guardar(It.IsAny<PlanDeCuentas>()), Times.Never);
        _mockAuditoriaRepo.Verify(r => r.Guardar(It.IsAny<Auditoria>()), Times.Never);
    }
}
