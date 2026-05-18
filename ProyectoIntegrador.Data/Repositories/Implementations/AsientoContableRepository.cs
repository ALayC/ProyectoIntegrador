using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class AsientoContableRepository : IAsientoContableRepository
{
    private readonly AppDbContext _context;

    public AsientoContableRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AsientoContable?> ObtenerPorId(Guid id)
    {
        return await _context.AsientosContables
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AsientoContable?> ObtenerPorIdConLineas(Guid id)
    {
        return await _context.AsientosContables
            .Include(a => a.LineasAsiento)
                .ThenInclude(l => l.CuentaContable)
            .Include(a => a.LineasAsiento)
                .ThenInclude(l => l.CentroCosto)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AsientoContable>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina)
    {
        return await _context.AsientosContables
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.Fecha)
            .ThenByDescending(a => a.Numero)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .Include(a => a.LineasAsiento)
            .ToListAsync();
    }

    public async Task<int> ContarPorCliente(Guid clienteId)
    {
        return await _context.AsientosContables
            .CountAsync(a => a.ClienteId == clienteId);
    }

    public async Task<List<AsientoContable>> ObtenerPorEjercicio(Guid clienteId, Guid ejercicioId, int pagina, int cantidadPorPagina)
    {
        return await _context.AsientosContables
            .Where(a => a.ClienteId == clienteId && a.EjercicioId == ejercicioId)
            .OrderByDescending(a => a.Fecha)
            .ThenByDescending(a => a.Numero)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .Include(a => a.LineasAsiento)
            .ToListAsync();
    }

    public async Task<int> ContarPorEjercicio(Guid clienteId, Guid ejercicioId)
    {
        return await _context.AsientosContables
            .CountAsync(a => a.ClienteId == clienteId && a.EjercicioId == ejercicioId);
    }

    public async Task<List<AsientoContable>> ObtenerPorRangoFecha(Guid clienteId, DateOnly fechaDesde, DateOnly fechaHasta, int pagina, int cantidadPorPagina)
    {
        return await _context.AsientosContables
            .Where(a => a.ClienteId == clienteId && a.Fecha >= fechaDesde && a.Fecha <= fechaHasta)
            .OrderByDescending(a => a.Fecha)
            .ThenByDescending(a => a.Numero)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .Include(a => a.LineasAsiento)
            .ToListAsync();
    }

    public async Task<int> ContarPorRangoFecha(Guid clienteId, DateOnly fechaDesde, DateOnly fechaHasta)
    {
        return await _context.AsientosContables
            .CountAsync(a => a.ClienteId == clienteId && a.Fecha >= fechaDesde && a.Fecha <= fechaHasta);
    }

    public async Task<int> ObtenerUltimoNumero(Guid clienteId, Guid ejercicioId)
    {
        return await _context.AsientosContables
            .Where(a => a.ClienteId == clienteId && a.EjercicioId == ejercicioId)
            .MaxAsync(a => (int?)a.Numero) ?? 0;
    }

    public async Task Guardar(AsientoContable asientoContable)
    {
        await _context.AsientosContables.AddAsync(asientoContable);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(AsientoContable asientoContable)
    {
        _context.AsientosContables.Update(asientoContable);
        await _context.SaveChangesAsync();
    }
}