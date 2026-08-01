using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class AuditoriaServiceTests
{
    private readonly Mock<IAuditoriaRepository> _auditoriaRepository;
    private readonly AuditoriaService _service;

    public AuditoriaServiceTests()
    {
        _auditoriaRepository = new Mock<IAuditoriaRepository>();
        _service = new AuditoriaService(_auditoriaRepository.Object, NullLogger<AuditoriaService>.Instance);
    }

    [Fact]
    public async Task Registrar_ConDatosValidos_GuardaConJsonSerializado()
    {
        var usuarioId = Guid.NewGuid();
        Auditoria? guardada = null;

        _auditoriaRepository
            .Setup(r => r.Guardar(It.IsAny<Auditoria>()))
            .Callback<Auditoria>(a => guardada = a)
            .Returns(Task.CompletedTask);

        await _service.Registrar(usuarioId, "Cliente", "Crear", new { Estado = "Anterior" }, new { Nombre = "Nuevo" });

        Assert.NotNull(guardada);
        Assert.Equal(usuarioId, guardada!.UsuarioId);
        Assert.Equal("Cliente", guardada.Entidad);
        Assert.Equal("Crear", guardada.Accion);
        Assert.Contains("estado", guardada.DatosAnteriores);
        Assert.Contains("nombre", guardada.DatosNuevos);

        _auditoriaRepository.Verify(r => r.Guardar(It.IsAny<Auditoria>()), Times.Once);
    }

    [Fact]
    public async Task Consultar_ConResultados_RetornaPaginadoMapeado()
    {
        var usuarioId = Guid.NewGuid();
        var auditoriaId = Guid.NewGuid();

        _auditoriaRepository
            .Setup(r => r.ObtenerFiltrado(usuarioId, "Cliente", "Crear", null, null, 1, 10))
            .ReturnsAsync(new List<Auditoria>
            {
                new()
                {
                    Id = auditoriaId,
                    UsuarioId = usuarioId,
                    Entidad = "Cliente",
                    Accion = "Crear",
                    FechaHora = DateTime.UtcNow,
                    DatosAnteriores = null,
                    DatosNuevos = "{\"nombre\":\"Empresa\"}",
                    Usuario = new Usuario { Id = usuarioId, NombreCompleto = "Admin Test", Email = "admin@test.com", Rol = new Rol { Nombre = "Administrador" } }
                }
            });

        _auditoriaRepository
            .Setup(r => r.ContarFiltrado(usuarioId, "Cliente", "Crear", null, null))
            .ReturnsAsync(1);

        var resultado = await _service.Consultar(usuarioId, "Cliente", "Crear", null, null, 1, 10);

        Assert.Single(resultado.Datos);
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal("Admin Test", resultado.Datos[0].UsuarioNombre);
        Assert.Equal("Cliente", resultado.Datos[0].Entidad);
    }
}
