using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class DispositivoConfiableRepository : IDispositivoConfiableRepository
{
    private readonly AppDbContext _context;

    public DispositivoConfiableRepository(AppDbContext context) => _context = context;

    public async Task<DispositivoConfiable?> ObtenerPorToken(string token)
    {
        return await _context.DispositivosConfiables
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.Token == token);
    }

    public async Task Guardar(DispositivoConfiable dispositivo)
    {
        await _context.DispositivosConfiables.AddAsync(dispositivo);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarExpiradosPorUsuario(Guid usuarioId)
    {
        var expirados = await _context.DispositivosConfiables
            .Where(d => d.UsuarioId == usuarioId && d.FechaExpiracion < DateTime.UtcNow)
            .ToListAsync();

        if (expirados.Count > 0)
        {
            _context.DispositivosConfiables.RemoveRange(expirados);
            await _context.SaveChangesAsync();
        }
    }
}
