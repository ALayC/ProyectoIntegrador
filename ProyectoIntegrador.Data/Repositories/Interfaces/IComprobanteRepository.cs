using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IComprobanteRepository
{
    Task<Comprobante?> ObtenerPorId(Guid id);
    Task<List<Comprobante>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCliente(Guid clienteId);
    Task<List<Comprobante>> ObtenerPorTipo(Guid clienteId, string tipo, int pagina, int cantidadPorPagina);
    Task<int> ContarPorTipo(Guid clienteId, string tipo);
    Task<List<Comprobante>> ObtenerPorPeriodo(Guid clienteId, DateTime fechaDesde, DateTime fechaHasta, int pagina, int cantidadPorPagina);
    Task<int> ContarPorPeriodo(Guid clienteId, DateTime fechaDesde, DateTime fechaHasta);
    Task<List<Comprobante>> ObtenerPorImportacion(Guid importacionId);
    Task<List<Comprobante>> ObtenerSinAsiento(Guid clienteId);
    Task Guardar(Comprobante comprobante);
    Task GuardarVarios(IEnumerable<Comprobante> comprobantes);
    Task Actualizar(Comprobante comprobante);
}
