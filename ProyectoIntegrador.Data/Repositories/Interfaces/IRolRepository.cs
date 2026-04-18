using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IRolRepository
{
    Task<Rol?> ObtenerPorId(Guid id);
    Task<Rol?> ObtenerPorNombre(string nombre);
    Task<List<Rol>> ObtenerTodos();
    Task Guardar(Rol rol);
    Task Actualizar(Rol rol);
    Task AsignarPermiso(Guid rolId, Guid permisoId);
    Task RemoverPermiso(Guid rolId, Guid permisoId);
    Task<List<Permiso>> ObtenerPermisos(Guid rolId);
}
