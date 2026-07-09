using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class InvitacionAuxiliarRepository : IInvitacionAuxiliarRepository
{
    private readonly AppDbContext _context;

    public InvitacionAuxiliarRepository(AppDbContext context) => _context = context;

    public async Task<InvitacionAuxiliar?> ObtenerPendientePorEmail(string email)
    {
        return await _context.InvitacionesAuxiliar
            .Include(i => i.Contador)
            .Where(i => i.Email == email
                     && i.Estado == "Pendiente"
                     && i.FechaExpiracion > DateTime.UtcNow)
            .OrderByDescending(i => i.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    public async Task<List<InvitacionAuxiliar>> ObtenerPorContador(Guid contadorId)
    {
        return await _context.InvitacionesAuxiliar
            .Where(i => i.ContadorId == contadorId)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();
    }

    public async Task Guardar(InvitacionAuxiliar invitacion)
    {
        _context.InvitacionesAuxiliar.Add(invitacion);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(InvitacionAuxiliar invitacion)
    {
        _context.InvitacionesAuxiliar.Update(invitacion);
        await _context.SaveChangesAsync();
    }
}
