using Microsoft.Extensions.Options;
using Moq;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
    private readonly Mock<IRolRepository> _mockRolRepo;
    private readonly Mock<ITokenRevocadoRepository> _mockTokenRepo;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUsuarioRepo = new Mock<IUsuarioRepository>();
        _mockRolRepo = new Mock<IRolRepository>();
_mockTokenRepo = new Mock<ITokenRevocadoRepository>();

    _jwtOptions = Options.Create(new JwtOptions
        {
  SecretKey = "ClaveSecretaParaTestsDeAlMenos32Caracteres!",
            Issuer = "TestIssuer",
        Audience = "TestAudience",
            DuracionMinutos = 60
    });

        _authService = new AuthService(
            _mockUsuarioRepo.Object,
  _mockRolRepo.Object,
            _mockTokenRepo.Object,
   _jwtOptions);
    }

    [Fact]
    public async Task Registrar_ConEmailNuevo_CreaUsuarioExitosamente()
    {
  // Arrange
  var registroDto = new RegistroDto
        {
            Email = "contador@test.com",
      Password = "Password123!",
        NombreCompleto = "Juan Pérez"
        };

        var rolContador = new Rol
   {
            Id = SeedData.RolContadorId,
 Nombre = "Contador",
            EsPredefinido = true
        };

        _mockUsuarioRepo
    .Setup(r => r.ExisteEmail(registroDto.Email))
            .ReturnsAsync(false);

        _mockRolRepo
            .Setup(r => r.ObtenerPorId(SeedData.RolContadorId))
            .ReturnsAsync(rolContador);

        _mockUsuarioRepo
            .Setup(r => r.Guardar(It.IsAny<Usuario>()))
          .Returns(Task.CompletedTask);

        // Act
        var resultado = await _authService.Registrar(registroDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.NotEmpty(resultado.Token);
        Assert.Equal(registroDto.Email, resultado.Email);
        Assert.Equal(registroDto.NombreCompleto, resultado.NombreCompleto);
        Assert.Equal("Contador", resultado.Rol);

        _mockUsuarioRepo.Verify(r => r.ExisteEmail(registroDto.Email), Times.Once);
    _mockUsuarioRepo.Verify(r => r.Guardar(It.Is<Usuario>(u =>
        u.Email == registroDto.Email &&
     u.NombreCompleto == registroDto.NombreCompleto &&
  u.ProveedorAuth == "Local" &&
 u.Estado == "Activo" &&
            u.RolId == SeedData.RolContadorId &&
       u.ContadorId == null &&
   !string.IsNullOrEmpty(u.PasswordHash)
   )), Times.Once);
    }

    [Fact]
    public async Task Registrar_ConEmailExistente_LanzaDuplicadoException()
    {
   // Arrange
        var registroDto = new RegistroDto
        {
     Email = "existente@test.com",
            Password = "Password123!",
            NombreCompleto = "María García"
        };

   _mockUsuarioRepo
   .Setup(r => r.ExisteEmail(registroDto.Email))
      .ReturnsAsync(true);

        // Act & Assert
    var exception = await Assert.ThrowsAsync<DuplicadoException>(
() => _authService.Registrar(registroDto));

        Assert.Contains("existente@test.com", exception.Message);

        _mockUsuarioRepo.Verify(r => r.ExisteEmail(registroDto.Email), Times.Once);
        _mockUsuarioRepo.Verify(r => r.Guardar(It.IsAny<Usuario>()), Times.Never);
    }
}
