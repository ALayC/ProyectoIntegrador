using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IPermisoRepository
{
    Task<Permiso?> ObtenerPorId(Guid id);
    Task<List<Permiso>> ObtenerTodos();
    Task<List<Permiso>> ObtenerPorModulo(string modulo);
    Task<List<Permiso>> ObtenerPorRol(Guid rolId);
    Task Guardar(Permiso permiso);
    Task Actualizar(Permiso permiso);
}
