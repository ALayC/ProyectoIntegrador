using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IEjercicioContableRepository
{
    Task<EjercicioContable?> ObtenerPorId(Guid id);
  Task<List<EjercicioContable>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorCliente(Guid clienteId);
Task<EjercicioContable?> ObtenerAbiertoPorCliente(Guid clienteId);
    Task<bool> ExisteSolapamiento(Guid clienteId, DateTime fechaInicio, DateTime fechaFin, Guid? excluirId = null);
    Task<EjercicioContable?> ObtenerPorFecha(Guid clienteId, DateTime fecha);
    Task Guardar(EjercicioContable ejercicioContable);
    Task Actualizar(EjercicioContable ejercicioContable);
}
