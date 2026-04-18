using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ILineaAsientoRepository
{
    Task<LineaAsiento?> ObtenerPorId(Guid id);
  Task<List<LineaAsiento>> ObtenerPorAsiento(Guid asientoId);
    Task<List<LineaAsiento>> ObtenerPorCuenta(Guid cuentaContableId, Guid ejercicioId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCuenta(Guid cuentaContableId, Guid ejercicioId);
    Task Guardar(LineaAsiento lineaAsiento);
    Task GuardarVarias(IEnumerable<LineaAsiento> lineas);
    Task Actualizar(LineaAsiento lineaAsiento);
}
