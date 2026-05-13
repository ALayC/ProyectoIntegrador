using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class EjercicioContableRepository : IEjercicioContableRepository
{
    private readonly AppDbContext _context;

    public EjercicioContableRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EjercicioContable?> ObtenerPorId(Guid id)
    {
        return await _context.EjerciciosContables
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<EjercicioContable>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina)
    {
        return await _context.EjerciciosContables
            .Where(e => e.ClienteId == clienteId)
            .OrderByDescending(e => e.FechaInicio)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .ToListAsync();
    }

    public async Task<int> ContarPorCliente(Guid clienteId)
    {
        return await _context.EjerciciosContables
            .CountAsync(e => e.ClienteId == clienteId);
    }

    public async Task<EjercicioContable?> ObtenerAbiertoPorCliente(Guid clienteId)
    {
        return await _context.EjerciciosContables
            .FirstOrDefaultAsync(e => e.ClienteId == clienteId && e.Estado == "Abierto");
    }

    public async Task<bool> ExisteSolapamiento(Guid clienteId, DateOnly fechaInicio, DateOnly fechaFin, Guid? excluirId = null)
    {
        var query = _context.EjerciciosContables
            .Where(e => e.ClienteId == clienteId)
            .Where(e => e.FechaInicio <= fechaFin && e.FechaFin >= fechaInicio);

        if (excluirId.HasValue)
        {
            query = query.Where(e => e.Id != excluirId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<EjercicioContable?> ObtenerPorFecha(Guid clienteId, DateOnly fecha)
    {
        return await _context.EjerciciosContables
            .FirstOrDefaultAsync(e => e.ClienteId == clienteId && e.FechaInicio <= fecha && e.FechaFin >= fecha);
    }

    public async Task Guardar(EjercicioContable ejercicioContable)
    {
        await _context.EjerciciosContables.AddAsync(ejercicioContable);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(EjercicioContable ejercicioContable)
    {
        _context.EjerciciosContables.Update(ejercicioContable);
        await _context.SaveChangesAsync();
    }
}
