using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class CuentaContableRepository : ICuentaContableRepository
{
    private readonly AppDbContext _context;

    public CuentaContableRepository(AppDbContext context) => _context = context;

    public async Task<CuentaContable?> ObtenerPorId(Guid id)
    {
        return await _context.CuentasContables
            .Include(c => c.CuentaPadre)
            .Include(c => c.CuentasHijas)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CuentaContable?> ObtenerPorCodigo(Guid planCuentasId, string codigo)
    {
        return await _context.CuentasContables
            .FirstOrDefaultAsync(c => c.PlanCuentasId == planCuentasId && c.Codigo == codigo);
    }

    public async Task<List<CuentaContable>> ObtenerPorPlanPaginado(Guid planCuentasId, int pagina, int cantidadPorPagina)
    {
        return await _context.CuentasContables
            .Where(c => c.PlanCuentasId == planCuentasId)
            .OrderBy(c => c.Codigo)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .ToListAsync();
    }

    public async Task<List<CuentaContable>> ObtenerTodasPorPlan(Guid planId)
    {
        return await _context.CuentasContables
            .Where(c => c.PlanCuentasId == planId)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<int> ContarPorPlanDeCuentas(Guid planCuentasId)
    {
        return await _context.CuentasContables.CountAsync(c => c.PlanCuentasId == planCuentasId);
    }

    public async Task<List<CuentaContable>> ObtenerHijas(Guid cuentaPadreId)
    {
        return await _context.CuentasContables
            .Where(c => c.CuentaPadreId == cuentaPadreId)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<List<CuentaContable>> ObtenerImputables(Guid planCuentasId)
    {
        return await _context.CuentasContables
            .Where(c => c.PlanCuentasId == planCuentasId && c.EsImputable)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<bool> ExisteCodigo(Guid planCuentasId, string codigo)
    {
        return await _context.CuentasContables.AnyAsync(c => c.PlanCuentasId == planCuentasId && c.Codigo == codigo);
    }

    public async Task<bool> TieneMovimientos(Guid cuentaContableId)
    {
        return await _context.LineasAsiento.AnyAsync(l => l.CuentaContableId == cuentaContableId);
    }

    public async Task Guardar(CuentaContable cuentaContable)
    {
        await _context.CuentasContables.AddAsync(cuentaContable);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(CuentaContable cuentaContable)
    {
        _context.CuentasContables.Update(cuentaContable);
        await _context.SaveChangesAsync();
    }
}
