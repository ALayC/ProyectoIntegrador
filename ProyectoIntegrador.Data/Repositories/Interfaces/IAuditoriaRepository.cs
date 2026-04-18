using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IAuditoriaRepository
{
    Task<Auditoria?> ObtenerPorId(Guid id);
    Task<List<Auditoria>> ObtenerPorUsuario(Guid usuarioId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorUsuario(Guid usuarioId);
    Task<List<Auditoria>> ObtenerPorEntidad(string entidad, int pagina, int cantidadPorPagina);
    Task<int> ContarPorEntidad(string entidad);
    Task<List<Auditoria>> ObtenerPorRangoFecha(DateTime fechaDesde, DateTime fechaHasta, int pagina, int cantidadPorPagina);
    Task<int> ContarPorRangoFecha(DateTime fechaDesde, DateTime fechaHasta);
    Task Guardar(Auditoria auditoria);
}
