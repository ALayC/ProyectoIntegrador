using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class RolRepository : IRolRepository
{
    private readonly AppDbContext _context;

    public RolRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> ObtenerPorId(Guid id)
    {
     return await _context.Roles
    .Include(r => r.RolPermisos)
         .ThenInclude(rp => rp.Permiso)
          .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rol?> ObtenerPorNombre(string nombre)
    {
        return await _context.Roles
   .Include(r => r.RolPermisos)
    .ThenInclude(rp => rp.Permiso)
    .FirstOrDefaultAsync(r => r.Nombre == nombre);
    }

    public async Task<List<Rol>> ObtenerTodos()
    {
        return await _context.Roles
        .OrderBy(r => r.Nombre)
       .ToListAsync();
    }

    public async Task Guardar(Rol rol)
    {
        await _context.Roles.AddAsync(rol);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(Rol rol)
    {
     _context.Roles.Update(rol);
        await _context.SaveChangesAsync();
    }

    public async Task AsignarPermiso(Guid rolId, Guid permisoId)
    {
        var existe = await _context.RolPermisos
            .AnyAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);

        if (!existe)
        {
     await _context.RolPermisos.AddAsync(new RolPermiso
     {
    RolId = rolId,
       PermisoId = permisoId
      });
            await _context.SaveChangesAsync();
        }
    }

 public async Task RemoverPermiso(Guid rolId, Guid permisoId)
  {
     var rolPermiso = await _context.RolPermisos
 .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);

        if (rolPermiso is not null)
  {
        _context.RolPermisos.Remove(rolPermiso);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Permiso>> ObtenerPermisos(Guid rolId)
    {
    return await _context.RolPermisos
   .Where(rp => rp.RolId == rolId)
 .Select(rp => rp.Permiso)
 .OrderBy(p => p.Modulo)
  .ThenBy(p => p.Accion)
            .ToListAsync();
    }
}
