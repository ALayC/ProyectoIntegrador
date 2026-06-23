using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;

namespace ProyectoIntegrador.Data.Repositories.Implementations
{
    public class LineaAsientoRepository : ILineaAsientoRepository

    {
        private readonly AppDbContext _context;

        public LineaAsientoRepository(AppDbContext context) => _context = context;

        public Task Actualizar(LineaAsiento lineaAsiento)
        {
            throw new NotImplementedException();
        }

        public Task<int> ContarPorCuenta(Guid cuentaContableId, Guid ejercicioId)
        {
            throw new NotImplementedException();
        }

        public Task Guardar(LineaAsiento lineaAsiento)
        {
            throw new NotImplementedException();
        }

        public Task GuardarVarias(IEnumerable<LineaAsiento> lineas)
        {
            throw new NotImplementedException();
        }

        public Task<List<LineaAsiento>> ObtenerPorAsiento(Guid asientoId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LineaAsiento>> ObtenerParaEstadoResultados(Guid clienteId, DateOnly fechaDesde, DateOnly fechaHasta)
        {
            return await _context.LineasAsiento
                .Include(l => l.Asiento)
                .Include(l => l.CuentaContable)
                .ThenInclude(c => c.CuentaPadre)
                .Where(l =>
                l.Asiento.ClienteId == clienteId &&
                l.Asiento.Estado == "Confirmado" &&
                l.Asiento.Fecha >= fechaDesde &&
                l.Asiento.Fecha <= fechaHasta &&
                (l.CuentaContable.Tipo == "Ingreso" ||
                l.CuentaContable.Tipo == "Egreso"))
                .ToListAsync();
        }

        public async Task<List<LineaAsiento>> ObtenerParaLiquidacionIva(Guid clienteId, DateOnly fechaDesde, DateOnly fechaHasta)
        {
            return await _context.LineasAsiento
                .Include(l => l.Asiento)
                .Include(l => l.CuentaContable)
                .Where(l =>
                    l.Asiento.ClienteId == clienteId &&
                    l.Asiento.Estado == "Confirmado" &&
                    l.Asiento.Fecha >= fechaDesde &&
                    l.Asiento.Fecha <= fechaHasta &&
                    (l.CuentaContable.Nombre == "IVA Crédito Fiscal" ||
                     l.CuentaContable.Nombre == "IVA Débito Fiscal"))
                .ToListAsync();
        }

        public Task<List<LineaAsiento>> ObtenerPorCuenta(Guid cuentaContableId, Guid ejercicioId, int pagina, int cantidadPorPagina)
        {
            throw new NotImplementedException();
        }

        public Task<LineaAsiento?> ObtenerPorId(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
