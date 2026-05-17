using System.Text.Json;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AuditoriaService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    /// <summary>
    /// Serializa los datos en JSON y registra el evento en la tabla de auditoría.
    /// </summary>
    public async Task Registrar(Guid usuarioId, string entidad, string accion, object? datosAnteriores, object? datosNuevos)
    {
        var auditoria = new Auditoria
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Entidad = entidad,
            Accion = accion,
            FechaHora = DateTime.UtcNow,
            DatosAnteriores = Serializar(datosAnteriores),
            DatosNuevos = Serializar(datosNuevos)
        };

        await _auditoriaRepository.Guardar(auditoria);
    }

    /// <summary>
    /// Serializa a JSON usando la convención definida para auditoría.
    /// </summary>
    private static string? Serializar(object? datos)
    {
        if (datos is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(datos, JsonOptions);
    }
}
