using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ICuentaContableRepository
{
    Task<CuentaContable?> ObtenerPorId(Guid id);
    Task<CuentaContable?> ObtenerPorCodigo(Guid planCuentasId, string codigo);
    Task<List<CuentaContable>> ObtenerPorPlanDeCuentas(Guid planCuentasId, int pagina, int cantidadPorPagina);
    Task<int> ContarPorPlanDeCuentas(Guid planCuentasId);
    Task<List<CuentaContable>> ObtenerHijas(Guid cuentaPadreId);
    Task<List<CuentaContable>> ObtenerImputables(Guid planCuentasId);
    Task<bool> ExisteCodigo(Guid planCuentasId, string codigo);
    Task<bool> TieneMovimientos(Guid cuentaContableId);
    Task Guardar(CuentaContable cuentaContable);
    Task Actualizar(CuentaContable cuentaContable);
}
