using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ITipoDeCambioRepository
{
    Task<TipoDeCambio?> ObtenerPorId(Guid id);
    Task<TipoDeCambio?> ObtenerPorMonedaYFecha(string moneda, DateTime fecha);
    Task<TipoDeCambio?> ObtenerUltimoPorMoneda(string moneda);
    Task<List<TipoDeCambio>> ObtenerPorRangoFecha(string moneda, DateTime fechaDesde, DateTime fechaHasta);
    Task<bool> ExisteParaMonedaYFecha(string moneda, DateTime fecha);
    Task Guardar(TipoDeCambio tipoDeCambio);
    Task GuardarVarios(IEnumerable<TipoDeCambio> tiposDeCambio);
    Task Actualizar(TipoDeCambio tipoDeCambio);
}
