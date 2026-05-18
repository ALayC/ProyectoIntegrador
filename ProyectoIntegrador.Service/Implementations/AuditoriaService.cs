using System.Text.Json;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
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
    /// Consulta registros de auditoría con filtros opcionales y paginación.
    /// </summary>
    public async Task<PaginadoDto<AuditoriaResponseDto>> Consultar(
        Guid? usuarioId,
        string? entidad,
        string? accion,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int cantidadPorPagina)
    {
        var registros = await _auditoriaRepository.ObtenerFiltrado(
            usuarioId, entidad, accion, fechaDesde, fechaHasta, pagina, cantidadPorPagina);

        var total = await _auditoriaRepository.ContarFiltrado(
            usuarioId, entidad, accion, fechaDesde, fechaHasta);

        var datos = registros.Select(Mapear).ToList();

        return new PaginadoDto<AuditoriaResponseDto>(datos, pagina, cantidadPorPagina, total);
    }

    // ??????????????????????????????????????????????
    // Métodos privados
    // ??????????????????????????????????????????????

    private static string? Serializar(object? datos)
    {
        if (datos is null)
            return null;

        return JsonSerializer.Serialize(datos, JsonOptions);
    }

    private static AuditoriaResponseDto Mapear(Auditoria a) => new()
    {
        Id = a.Id,
        UsuarioId = a.UsuarioId,
        UsuarioNombre = a.Usuario?.NombreCompleto ?? string.Empty,
        Entidad = a.Entidad,
        Accion = a.Accion,
        FechaHora = a.FechaHora,
        DatosAnteriores = a.DatosAnteriores,
        DatosNuevos = a.DatosNuevos
    };
}
