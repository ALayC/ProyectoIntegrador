using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class ComprobanteRepository : IComprobanteRepository
{
    private readonly AppDbContext _context;

    public ComprobanteRepository(AppDbContext context) => _context = context;

    public async Task<Comprobante?> ObtenerPorId(Guid id)
    {
        return await _context.Comprobantes
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Comprobante>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina)
    {
        return await _context.Comprobantes
            .Where(c => c.ClienteId == clienteId)
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .ToListAsync();
    }

    public async Task<List<Comprobante>> ObtenerPorFiltros(
        Guid clienteId,
        TipoComprobante? tipo,
        string? rut,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        EstadoComprobante? estado,
        int pagina,
        int cantidadPorPagina)
    {
        var query = _context.Comprobantes
            .Where(c => c.ClienteId == clienteId)
            .AsQueryable();

        if (tipo.HasValue)
            query = query.Where(c => c.Tipo == tipo.Value);

        if (!string.IsNullOrWhiteSpace(rut))
            query = query.Where(c => c.RUT == rut);

        if (fechaDesde.HasValue)
            query = query.Where(c => c.Fecha >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(c => c.Fecha <= fechaHasta.Value);

        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado.Value);

        return await query
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .ToListAsync();
    }

    public async Task<bool> ExisteDuplicado(string numero, string rut, DateOnly fecha, Guid clienteId)
    {
        return await _context.Comprobantes.AnyAsync(c =>
            c.ClienteId == clienteId &&
            c.Numero == numero &&
            c.RUT == rut &&
            c.Fecha == fecha &&
            c.DeletedAt == null);
    }

    public async Task<Comprobante?> ObtenerPorAsiento(Guid asientoId)
    {
        return await _context.Comprobantes
            .FirstOrDefaultAsync(c => c.AsientoId == asientoId);
    }

    public async Task Guardar(Comprobante comprobante)
    {
        await _context.Comprobantes.AddAsync(comprobante);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(Comprobante comprobante)
    {
        _context.Comprobantes.Update(comprobante);
        await _context.SaveChangesAsync();
    }

    public async Task Anular(Guid comprobanteId)
    {
        var comprobante = await _context.Comprobantes
            .FirstOrDefaultAsync(c => c.Id == comprobanteId);

        if (comprobante is null)
            return;

        comprobante.Estado = EstadoComprobante.Anulado;
        comprobante.DeletedAt = DateTime.UtcNow;
        comprobante.UpdatedAt = DateTime.UtcNow;

        _context.Comprobantes.Update(comprobante);
        await _context.SaveChangesAsync();
    }
}
