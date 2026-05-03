using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class PermisoRepository : IPermisoRepository
{
    private readonly AppDbContext _context;

    public PermisoRepository(AppDbContext context)
    {
 _context = context;
    }

    public async Task<Permiso?> ObtenerPorId(Guid id)
   => await _context.Permisos.FindAsync(id);

    public async Task<List<Permiso>> ObtenerTodos()
  => await _context.Permisos
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Accion)
            .ToListAsync();

    public async Task<List<Permiso>> ObtenerPorModulo(string modulo)
        => await _context.Permisos
      .Where(p => p.Modulo == modulo)
            .OrderBy(p => p.Accion)
            .ToListAsync();

    public async Task<List<Permiso>> ObtenerPorRol(Guid rolId)
        => await _context.RolPermisos
            .Where(rp => rp.RolId == rolId)
            .Select(rp => rp.Permiso)
        .OrderBy(p => p.Modulo)
        .ThenBy(p => p.Accion)
            .ToListAsync();

    public async Task Guardar(Permiso permiso)
    {
        await _context.Permisos.AddAsync(permiso);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(Permiso permiso)
    {
  _context.Permisos.Update(permiso);
      await _context.SaveChangesAsync();
    }
}
