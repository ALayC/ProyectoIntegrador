using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObtenerPorId(Guid id)
  {
        return await _context.Clientes
    .Include(c => c.Contador)
        .Include(c => c.PlanDeCuentas)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> ObtenerPorRut(string rut)
    {
        return await _context.Clientes
  .Include(c => c.Contador)
            .FirstOrDefaultAsync(c => c.Rut == rut);
    }

    public async Task<List<Cliente>> ObtenerPorContador(Guid contadorId, int pagina, int cantidadPorPagina)
    {
    return await _context.Clientes
            .Where(c => c.ContadorId == contadorId)
            .OrderBy(c => c.RazonSocial)
      .Skip((pagina - 1) * cantidadPorPagina)
       .Take(cantidadPorPagina)
          .ToListAsync();
    }

    public async Task<int> ContarPorContador(Guid contadorId)
    {
 return await _context.Clientes
       .CountAsync(c => c.ContadorId == contadorId);
    }

    public async Task<bool> ExisteRut(string rut)
    {
        return await _context.Clientes.AnyAsync(c => c.Rut == rut);
    }

    public async Task Guardar(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(Cliente cliente)
    {
 _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }
}
