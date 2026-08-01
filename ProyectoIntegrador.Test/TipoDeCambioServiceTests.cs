using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Implementations;

namespace ProyectoIntegrador.Test;

public class TipoDeCambioServiceTests
{
    private readonly Mock<ITipoDeCambioRepository> _repository;
    private readonly TipoDeCambioService _service;

    public TipoDeCambioServiceTests()
    {
        _repository = new Mock<ITipoDeCambioRepository>();

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(f => f.CreateClient("BCU"))
            .Returns(new HttpClient(new StubHttpMessageHandler()));

        _service = new TipoDeCambioService(
            _repository.Object,
            httpClientFactory.Object,
            NullLogger<TipoDeCambioService>.Instance);
    }

    [Fact]
    public async Task ObtenerCotizacionDetalle_ConDatoEnRepositorio_RetornaValorExistente()
    {
        var fecha = new DateOnly(2026, 6, 1);

        _repository
            .Setup(r => r.ObtenerPorMonedaYFecha("USD", fecha))
            .ReturnsAsync(new TipoDeCambio
            {
                Id = Guid.NewGuid(),
                Moneda = "USD",
                Fecha = fecha,
                Valor = 40.25m,
                FuenteOrigen = "Manual"
            });

        var resultado = await _service.ObtenerCotizacionDetalle("usd", fecha);

        Assert.Equal(40.25m, resultado.Valor);
        Assert.Equal(fecha, resultado.FechaReal);
    }

    [Fact]
    public async Task ObtenerUltimoTipoCambioVenta_ConDatoEnRepositorio_RetornaValor()
    {
        _repository
            .Setup(r => r.ObtenerUltimoPorMoneda("USD"))
            .ReturnsAsync(new TipoDeCambio
            {
                Id = Guid.NewGuid(),
                Moneda = "USD",
                Fecha = new DateOnly(2026, 6, 2),
                Valor = 40.8m,
                FuenteOrigen = "Manual"
            });

        var resultado = await _service.ObtenerUltimoTipoCambioVenta("usd");

        Assert.Equal(40.8m, resultado);
    }

    [Fact]
    public async Task SincronizarDesdeBCU_ConMonedaNoSoportada_NoGuardaRegistros()
    {
        await _service.SincronizarDesdeBCU("EUR", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        _repository.Verify(r => r.GuardarVarios(It.IsAny<IEnumerable<TipoDeCambio>>()), Times.Never);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            });
        }
    }
}
