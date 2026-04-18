using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IAsientoContableRepository
{
    Task<AsientoContable?> ObtenerPorId(Guid id);
    Task<AsientoContable?> ObtenerPorIdConLineas(Guid id);
    Task<List<AsientoContable>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCliente(Guid clienteId);
    Task<List<AsientoContable>> ObtenerPorEjercicio(Guid clienteId, Guid ejercicioId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorEjercicio(Guid clienteId, Guid ejercicioId);
    Task<List<AsientoContable>> ObtenerPorRangoFecha(Guid clienteId, DateTime fechaDesde, DateTime fechaHasta, int pagina, int cantidadPorPagina);
    Task<int> ContarPorRangoFecha(Guid clienteId, DateTime fechaDesde, DateTime fechaHasta);
    Task<int> ObtenerUltimoNumero(Guid clienteId, Guid ejercicioId);
    Task Guardar(AsientoContable asientoContable);
    Task Actualizar(AsientoContable asientoContable);
}
