using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IPlanDeCuentasRepository
{
    Task<PlanDeCuentas?> ObtenerPorId(Guid id);
    Task<PlanDeCuentas?> ObtenerPorClienteId(Guid clienteId);
    Task Guardar(PlanDeCuentas planDeCuentas);
    Task Actualizar(PlanDeCuentas planDeCuentas);
}
