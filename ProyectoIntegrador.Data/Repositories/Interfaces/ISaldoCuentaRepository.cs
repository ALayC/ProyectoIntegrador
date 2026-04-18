using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ISaldoCuentaRepository
{
    Task<SaldoCuenta?> ObtenerPorId(Guid id);
    Task<SaldoCuenta?> ObtenerPorCuentaYPeriodo(Guid clienteId, Guid cuentaContableId, Guid ejercicioId, string periodo);
    Task<List<SaldoCuenta>> ObtenerPorEjercicio(Guid clienteId, Guid ejercicioId);
    Task<List<SaldoCuenta>> ObtenerPorCuenta(Guid clienteId, Guid cuentaContableId, Guid ejercicioId);
    Task Guardar(SaldoCuenta saldoCuenta);
    Task GuardarVarios(IEnumerable<SaldoCuenta> saldos);
    Task Actualizar(SaldoCuenta saldoCuenta);
}
