using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorId(Guid id);
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<Usuario?> ObtenerPorTokenConfirmacion(string token);
    Task<Usuario?> ObtenerPorTokenRestablecimiento(string token);
    Task<List<Usuario>> ObtenerAuxiliaresPorContador(Guid contadorId, int pagina, int cantidadPorPagina);
    Task<int> ContarAuxiliaresPorContador(Guid contadorId);
    Task<List<Usuario>> ObtenerTodos(int pagina, int cantidadPorPagina);
    Task<List<Usuario>> ObtenerTodos();
    Task<int> ContarTodos();
    Task<bool> ExisteEmail(string email);
    Task Guardar(Usuario usuario);
    Task Actualizar(Usuario usuario);
}
