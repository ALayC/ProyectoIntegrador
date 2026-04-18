using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IImportacionRepository
{
    Task<Importacion?> ObtenerPorId(Guid id);
    Task<List<Importacion>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCliente(Guid clienteId);
    Task<List<Importacion>> ObtenerPorUsuario(Guid usuarioId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorUsuario(Guid usuarioId);
 Task Guardar(Importacion importacion);
    Task Actualizar(Importacion importacion);
}
