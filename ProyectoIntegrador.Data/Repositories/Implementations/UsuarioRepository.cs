using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorId(Guid id)
    {
        return await _context.Usuarios
          .Include(u => u.Rol)
     .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorEmail(string email)
    {
        return await _context.Usuarios
       .Include(u => u.Rol)
             .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<Usuario>> ObtenerAuxiliaresPorContador(Guid contadorId, int pagina, int cantidadPorPagina)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.ContadorId == contadorId)
            .OrderBy(u => u.NombreCompleto)
            .Skip((pagina - 1) * cantidadPorPagina)
    .Take(cantidadPorPagina)
     .ToListAsync();
    }

    public async Task<int> ContarAuxiliaresPorContador(Guid contadorId)
    {
        return await _context.Usuarios
 .CountAsync(u => u.ContadorId == contadorId);
    }

    public async Task<List<Usuario>> ObtenerTodos(int pagina, int cantidadPorPagina)
    {
        return await _context.Usuarios
         .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
 .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
    .ToListAsync();
    }

    public async Task<int> ContarTodos()
    {
        return await _context.Usuarios.CountAsync();
    }

    public async Task<bool> ExisteEmail(string email)
    {
        return await _context.Usuarios.AnyAsync(u => u.Email == email);
    }

    public async Task Guardar(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task Actualizar(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
