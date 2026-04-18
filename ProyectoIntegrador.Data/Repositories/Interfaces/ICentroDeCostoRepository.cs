using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ICentroDeCostoRepository
{
    Task<CentroDeCosto?> ObtenerPorId(Guid id);
    Task<CentroDeCosto?> ObtenerPorCodigo(Guid clienteId, string codigo);
    Task<List<CentroDeCosto>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCliente(Guid clienteId);
    Task<List<CentroDeCosto>> ObtenerActivosPorCliente(Guid clienteId);
    Task<bool> ExisteCodigo(Guid clienteId, string codigo);
    Task<bool> TieneMovimientos(Guid centroCostoId);
    Task Guardar(CentroDeCosto centroDeCosto);
    Task Actualizar(CentroDeCosto centroDeCosto);
}
