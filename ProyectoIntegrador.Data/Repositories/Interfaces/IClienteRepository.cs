using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorId(Guid id);
    Task<Cliente?> ObtenerPorRut(string rut);
  Task<List<Cliente>> ObtenerPorContador(Guid contadorId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorContador(Guid contadorId);
    Task<bool> ExisteRut(string rut);
    Task Guardar(Cliente cliente);
    Task Actualizar(Cliente cliente);
}
