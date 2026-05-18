using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ISaldoCuentaRepository
{
    Task<SaldoCuenta?> ObtenerPorPeriodo(Guid clienteId, Guid cuentaContableId, Guid ejercicioId, DateOnly periodo);
    Task Guardar(SaldoCuenta saldoCuenta);
    Task Actualizar(SaldoCuenta saldoCuenta);
}
