namespace ProyectoIntegrador.Service.Interfaces;

public record CotizacionResult(decimal Valor, DateOnly FechaReal);

public interface ITipoDeCambioService
{
    Task<decimal> ObtenerTipoCambioVenta(string moneda, DateOnly fecha);
    Task<CotizacionResult> ObtenerCotizacionDetalle(string moneda, DateOnly fecha);
    Task<decimal> ObtenerUltimoTipoCambioVenta(string moneda);
    Task SincronizarDesdeBCU(string moneda, DateOnly fechaDesde, DateOnly fechaHasta);
}
