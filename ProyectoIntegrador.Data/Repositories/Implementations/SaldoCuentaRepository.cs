using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class SaldoCuentaRepository : ISaldoCuentaRepository
{
    private readonly AppDbContext _context;

    public SaldoCuentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoCuenta?> ObtenerPorPeriodo(Guid clienteId, Guid cuentaContableId, Guid ejercicioId, DateOnly periodo)
    {
        return await _context.SaldosCuenta
            .FirstOrDefaultAsync(s =>
                s.ClienteId == clienteId &&
                s.CuentaContableId == cuentaContableId &&
                s.EjercicioId == ejercicioId &&
                s.Periodo == periodo);
    }

    public async Task Guardar(SaldoCuenta saldoCuenta)
    {
        await _context.SaldosCuenta.AddAsync(saldoCuenta);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(SaldoCuenta saldoCuenta)
    {
        _context.SaldosCuenta.Update(saldoCuenta);
        await _context.SaveChangesAsync();
    }
}