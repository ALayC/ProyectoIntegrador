using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Test;

public class AuxiliarServiceTests
{
    private readonly Mock<IInvitacionAuxiliarRepository> _invitacionRepository;
    private readonly Mock<IUsuarioRepository> _usuarioRepository;
    private readonly Mock<IEmailService> _emailService;
    private readonly AuxiliarService _service;

    public AuxiliarServiceTests()
    {
        _invitacionRepository = new Mock<IInvitacionAuxiliarRepository>();
        _usuarioRepository = new Mock<IUsuarioRepository>();
        _emailService = new Mock<IEmailService>();

        var options = Options.Create(new UIOptions { BaseUrl = "https://ui.test" });

        _service = new AuxiliarService(
            _invitacionRepository.Object,
            _usuarioRepository.Object,
            _emailService.Object,
            options,
            NullLogger<AuxiliarService>.Instance);
    }

    [Fact]
    public async Task InvitarAuxiliar_ConContadorValido_GuardaInvitacionYEnviaEmail()
    {
        var contadorId = Guid.NewGuid();
        var dto = new InvitarAuxiliarDto { Email = "Auxiliar@Test.com" };

        _usuarioRepository
            .Setup(r => r.ObtenerPorId(contadorId))
            .ReturnsAsync(new Usuario
            {
                Id = contadorId,
                NombreCompleto = "Contador Demo",
                Email = "contador@test.com",
                Rol = new Rol { Nombre = "Contador" }
            });

        _usuarioRepository
            .Setup(r => r.ObtenerPorEmail(dto.Email))
            .ReturnsAsync((Usuario?)null);

        InvitacionAuxiliar? guardada = null;
        _invitacionRepository
            .Setup(r => r.Guardar(It.IsAny<InvitacionAuxiliar>()))
            .Callback<InvitacionAuxiliar>(i => guardada = i)
            .Returns(Task.CompletedTask);

        _emailService
            .Setup(e => e.EnviarInvitacionAuxiliarAsync(dto.Email, "Contador Demo", "https://ui.test"))
            .Returns(Task.CompletedTask);

        var resultado = await _service.InvitarAuxiliar(contadorId, dto);

        Assert.NotNull(guardada);
        Assert.Equal("auxiliar@test.com", guardada!.Email);
        Assert.Equal("Pendiente", resultado.Estado);

        _invitacionRepository.Verify(r => r.Guardar(It.IsAny<InvitacionAuxiliar>()), Times.Once);
        _emailService.Verify(e => e.EnviarInvitacionAuxiliarAsync(dto.Email, "Contador Demo", "https://ui.test"), Times.Once);
    }

    [Fact]
    public async Task InvitarAuxiliar_ConRolSinPermiso_LanzaAccesoNoAutorizadoException()
    {
        var contadorId = Guid.NewGuid();

        _usuarioRepository
            .Setup(r => r.ObtenerPorId(contadorId))
            .ReturnsAsync(new Usuario
            {
                Id = contadorId,
                NombreCompleto = "Usuario Sin Permiso",
                Email = "usuario@test.com",
                Rol = new Rol { Nombre = "Auxiliar" }
            });

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            _service.InvitarAuxiliar(contadorId, new InvitarAuxiliarDto { Email = "nuevo@test.com" }));

        _invitacionRepository.Verify(r => r.Guardar(It.IsAny<InvitacionAuxiliar>()), Times.Never);
    }

    [Fact]
    public async Task RevocarAuxiliar_CuandoNoPerteneceAlContador_LanzaAccesoNoAutorizadoException()
    {
        var contadorId = Guid.NewGuid();
        var auxiliarId = Guid.NewGuid();

        _usuarioRepository
            .Setup(r => r.ObtenerPorId(auxiliarId))
            .ReturnsAsync(new Usuario
            {
                Id = auxiliarId,
                Email = "auxiliar@test.com",
                ContadorId = Guid.NewGuid(),
                Rol = new Rol { Nombre = "Auxiliar" }
            });

        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            _service.RevocarAuxiliar(contadorId, auxiliarId));

        _usuarioRepository.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
    }
}
