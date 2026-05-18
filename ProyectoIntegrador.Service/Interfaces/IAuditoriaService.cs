using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IAuditoriaService
{
    /// <summary>
    /// Registra un evento de auditoría serializando los datos anteriores y nuevos a JSON.
    /// </summary>
    Task Registrar(Guid usuarioId, string entidad, string accion, object? datosAnteriores, object? datosNuevos);

    /// <summary>
    /// Consulta registros de auditoría con filtros opcionales y paginación.
    /// Solo accesible para Administrador.
    /// </summary>
    Task<PaginadoDto<AuditoriaResponseDto>> Consultar(
        Guid? usuarioId,
        string? entidad,
        string? accion,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int cantidadPorPagina);
}
