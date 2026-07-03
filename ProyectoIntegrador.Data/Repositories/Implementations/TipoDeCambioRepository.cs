using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class TipoDeCambioRepository : ITipoDeCambioRepository
{
    private readonly AppDbContext _context;

    public TipoDeCambioRepository(AppDbContext context) => _context = context;

    public async Task<TipoDeCambio?> ObtenerPorId(Guid id)
        => await _context.TiposDeCambio.FindAsync(id);

    public async Task<TipoDeCambio?> ObtenerPorMonedaYFecha(string moneda, DateOnly fecha)
        => await _context.TiposDeCambio
            .FirstOrDefaultAsync(t => t.Moneda == moneda && t.Fecha == fecha);

    public async Task<TipoDeCambio?> ObtenerUltimoPorMoneda(string moneda)
        => await _context.TiposDeCambio
            .Where(t => t.Moneda == moneda)
            .OrderByDescending(t => t.Fecha)
            .FirstOrDefaultAsync();

    public async Task<List<TipoDeCambio>> ObtenerPorRangoFecha(
        string moneda, DateOnly fechaDesde, DateOnly fechaHasta)
        => await _context.TiposDeCambio
            .Where(t => t.Moneda == moneda && t.Fecha >= fechaDesde && t.Fecha <= fechaHasta)
            .OrderBy(t => t.Fecha)
            .ToListAsync();

    public async Task<bool> ExisteParaMonedaYFecha(string moneda, DateOnly fecha)
        => await _context.TiposDeCambio
            .AnyAsync(t => t.Moneda == moneda && t.Fecha == fecha);

    public async Task Guardar(TipoDeCambio tipoDeCambio)
    {
        _context.TiposDeCambio.Add(tipoDeCambio);
        await _context.SaveChangesAsync();
    }

    public async Task GuardarVarios(IEnumerable<TipoDeCambio> tiposDeCambio)
    {
        _context.TiposDeCambio.AddRange(tiposDeCambio);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(TipoDeCambio tipoDeCambio)
    {
        _context.TiposDeCambio.Update(tipoDeCambio);
        await _context.SaveChangesAsync();
    }
}
