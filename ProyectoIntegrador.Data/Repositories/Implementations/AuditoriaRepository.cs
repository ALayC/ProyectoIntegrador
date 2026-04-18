using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AppDbContext _context;

    public AuditoriaRepository(AppDbContext context)
    {
 _context = context;
    }

    public async Task<Auditoria?> ObtenerPorId(Guid id)
    {
        return await _context.Auditorias
         .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Auditoria>> ObtenerPorUsuario(Guid usuarioId, int pagina, int cantidadPorPagina)
    {
return await _context.Auditorias
     .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.FechaHora)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
   .ToListAsync();
    }

 public async Task<int> ContarPorUsuario(Guid usuarioId)
    {
        return await _context.Auditorias.CountAsync(a => a.UsuarioId == usuarioId);
    }

    public async Task<List<Auditoria>> ObtenerPorEntidad(string entidad, int pagina, int cantidadPorPagina)
    {
  return await _context.Auditorias
     .Where(a => a.Entidad == entidad)
 .OrderByDescending(a => a.FechaHora)
 .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
       .ToListAsync();
  }

    public async Task<int> ContarPorEntidad(string entidad)
    {
 return await _context.Auditorias.CountAsync(a => a.Entidad == entidad);
    }

    public async Task<List<Auditoria>> ObtenerPorRangoFecha(DateTime fechaDesde, DateTime fechaHasta, int pagina, int cantidadPorPagina)
    {
 return await _context.Auditorias
 .Where(a => a.FechaHora >= fechaDesde && a.FechaHora <= fechaHasta)
    .OrderByDescending(a => a.FechaHora)
    .Skip((pagina - 1) * cantidadPorPagina)
        .Take(cantidadPorPagina)
   .ToListAsync();
    }

    public async Task<int> ContarPorRangoFecha(DateTime fechaDesde, DateTime fechaHasta)
    {
        return await _context.Auditorias
   .CountAsync(a => a.FechaHora >= fechaDesde && a.FechaHora <= fechaHasta);
    }

    public async Task Guardar(Auditoria auditoria)
    {
        await _context.Auditorias.AddAsync(auditoria);
        await _context.SaveChangesAsync();
    }
}
