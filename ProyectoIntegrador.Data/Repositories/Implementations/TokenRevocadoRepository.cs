using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class TokenRevocadoRepository : ITokenRevocadoRepository
{
    private readonly AppDbContext _context;

    public TokenRevocadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TokenRevocado?> ObtenerPorId(Guid id)
    {
        return await _context.TokensRevocados.FindAsync(id);
    }

    public async Task<bool> EstaRevocado(string token)
    {
  return await _context.TokensRevocados.AnyAsync(t => t.Token == token);
    }

    public async Task Guardar(TokenRevocado tokenRevocado)
    {
        await _context.TokensRevocados.AddAsync(tokenRevocado);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarExpirados()
    {
        var ahora = DateTime.UtcNow;
     var expirados = await _context.TokensRevocados
  .Where(t => t.ExpiraEn <= ahora)
       .ToListAsync();

        if (expirados.Count > 0)
  {
      _context.TokensRevocados.RemoveRange(expirados);
   await _context.SaveChangesAsync();
        }
    }
}
