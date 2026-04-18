using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class PlanDeCuentasRepository : IPlanDeCuentasRepository
{
    private readonly AppDbContext _context;

    public PlanDeCuentasRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlanDeCuentas?> ObtenerPorId(Guid id)
    {
        return await _context.PlanesDeCuentas
            .Include(p => p.CuentasContables)
  .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlanDeCuentas?> ObtenerPorClienteId(Guid clienteId)
    {
        return await _context.PlanesDeCuentas
    .Include(p => p.CuentasContables)
     .FirstOrDefaultAsync(p => p.ClienteId == clienteId);
    }

    public async Task Guardar(PlanDeCuentas planDeCuentas)
    {
        await _context.PlanesDeCuentas.AddAsync(planDeCuentas);
     await _context.SaveChangesAsync();
    }

    public async Task Actualizar(PlanDeCuentas planDeCuentas)
    {
        _context.PlanesDeCuentas.Update(planDeCuentas);
        await _context.SaveChangesAsync();
    }
}
