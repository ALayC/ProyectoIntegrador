using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IComprobanteRepository
{
    Task<Comprobante?> ObtenerPorId(Guid id);
    Task<List<Comprobante>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<List<Comprobante>> ObtenerPorFiltros(
        Guid clienteId,
        TipoComprobante? tipo,
        string? rut,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        EstadoComprobante? estado,
        int pagina,
        int cantidadPorPagina);
    Task<bool> ExisteDuplicado(string numero, string rut, DateOnly fecha, Guid clienteId);
    Task<Comprobante?> ObtenerPorAsiento(Guid asientoId);
    Task Guardar(Comprobante comprobante);
    Task Actualizar(Comprobante comprobante);
    Task Anular(Guid comprobanteId);
}
