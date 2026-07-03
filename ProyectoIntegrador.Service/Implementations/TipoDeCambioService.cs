using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class TipoDeCambioService : ITipoDeCambioService
{
    private const string BcuWsdlUrl = "https://cotizaciones.bcu.gub.uy/wscotizaciones/servlet/awsbcucotizaciones";
    private const int CodigoUsd = 2225;

    private static readonly Dictionary<string, int> CodigosMoneda = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 2225
    };

    private readonly ITipoDeCambioRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TipoDeCambioService> _logger;

    public TipoDeCambioService(
        ITipoDeCambioRepository repository,
        IHttpClientFactory httpClientFactory,
        ILogger<TipoDeCambioService> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ── Obtener TC para una moneda y fecha concretas ─────────────────────

    public async Task<decimal> ObtenerTipoCambioVenta(string moneda, DateOnly fecha)
        => (await ObtenerCotizacionDetalle(moneda, fecha)).Valor;

    // ── Obtener TC con la fecha real de la cotización ─────────────────────

    public async Task<CotizacionResult> ObtenerCotizacionDetalle(string moneda, DateOnly fecha)
    {
        moneda = moneda.ToUpperInvariant();

        // 1. Buscar en BD (día exacto)
        var existente = await _repository.ObtenerPorMonedaYFecha(moneda, fecha);
        if (existente is not null)
            return new CotizacionResult(existente.Valor, existente.Fecha);

        // 2. Consultar BCU con rango de 7 días hacia atrás para cubrir fines de semana/feriados
        var resultado = await ConsultarBCU(moneda, fecha.AddDays(-7), fecha);
        if (resultado is not null)
        {
            await GuardarSiNoExiste(moneda, resultado.Value.Fecha, resultado.Value.Valor, "BCU");
            return new CotizacionResult(resultado.Value.Valor, resultado.Value.Fecha);
        }

        // 3. Fallback: último disponible en BD
        _logger.LogWarning(
            "BCU no devolvió cotización para {Moneda} en {Fecha}. Usando último valor disponible.",
            moneda, fecha);

        var ultimo = await _repository.ObtenerUltimoPorMoneda(moneda);
        if (ultimo is not null)
            return new CotizacionResult(ultimo.Valor, ultimo.Fecha);

        _logger.LogError("No hay ningún tipo de cambio disponible para {Moneda}.", moneda);
        return new CotizacionResult(1m, fecha);
    }

    // ── Obtener el TC más reciente sin importar la fecha ─────────────────

    public async Task<decimal> ObtenerUltimoTipoCambioVenta(string moneda)
    {
        moneda = moneda.ToUpperInvariant();

        var ultimo = await _repository.ObtenerUltimoPorMoneda(moneda);
        if (ultimo is not null)
            return ultimo.Valor;

        // Nada en BD → consultar BCU con rango de 7 días para cubrir fines de semana
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var resultado = await ConsultarBCU(moneda, hoy.AddDays(-7), hoy);
        if (resultado is not null)
        {
            await GuardarSiNoExiste(moneda, resultado.Value.Fecha, resultado.Value.Valor, "BCU");
            return resultado.Value.Valor;
        }

        return 1m;
    }

    // ── Sincronizar rango de fechas desde BCU ────────────────────────────

    public async Task SincronizarDesdeBCU(string moneda, DateOnly fechaDesde, DateOnly fechaHasta)
    {
        moneda = moneda.ToUpperInvariant();

        if (!CodigosMoneda.TryGetValue(moneda, out var codigoMoneda))
        {
            _logger.LogWarning("Moneda {Moneda} no soportada para sincronización BCU.", moneda);
            return;
        }

        var soapEnvelope = BuildSoapEnvelope(codigoMoneda, fechaDesde, fechaHasta);
        var xmlResponse = await PostSoap(soapEnvelope);
        if (xmlResponse is null)
        {
            _logger.LogWarning("BCU no respondió durante la sincronización {Moneda} {Desde}-{Hasta}.",
                moneda, fechaDesde, fechaHasta);
            return;
        }

        var registros = ParsearRespuestaBCU(xmlResponse, moneda);
        var nuevos = new List<TipoDeCambio>();

        foreach (var (fecha, valor) in registros)
        {
            if (!await _repository.ExisteParaMonedaYFecha(moneda, fecha))
            {
                nuevos.Add(new TipoDeCambio
                {
                    Id = Guid.NewGuid(),
                    Moneda = moneda,
                    Fecha = fecha,
                    Valor = valor,
                    FuenteOrigen = "BCU"
                });
            }
        }

        if (nuevos.Count > 0)
        {
            await _repository.GuardarVarios(nuevos);
            _logger.LogInformation(
                "Sincronización BCU: {Count} registros guardados para {Moneda} ({Desde} – {Hasta}).",
                nuevos.Count, moneda, fechaDesde, fechaHasta);
        }
    }

    // ── SOAP helpers ─────────────────────────────────────────────────────

    private async Task<(DateOnly Fecha, decimal Valor)?> ConsultarBCU(string moneda, DateOnly fechaDesde, DateOnly fechaHasta)
    {
        if (!CodigosMoneda.TryGetValue(moneda, out var codigoMoneda))
            return null;

        var soapEnvelope = BuildSoapEnvelope(codigoMoneda, fechaDesde, fechaHasta);
        var xmlResponse = await PostSoap(soapEnvelope);
        if (xmlResponse is null)
            return null;

        var registros = ParsearRespuestaBCU(xmlResponse, moneda);

        // Lista vacía → BCU no tiene datos para ese rango (feriado, fin de semana, etc.)
        if (registros.Count == 0)
            return null;

        // Devolver el más cercano a fechaHasta
        return registros.OrderByDescending(r => r.Fecha).First();
    }

    private static string BuildSoapEnvelope(int codigoMoneda, DateOnly fechaDesde, DateOnly fechaHasta)
    {
        // WSDL: document/literal, targetNamespace="Cotiza"
        // Fechas como xsd:date → formato ISO yyyy-MM-dd
        // SOAPAction: Cotizaaction/AWSBCUCOTIZACIONES.Execute
        // elementFormDefault="qualified" → todos los elementos llevan prefijo tns:
        var desde = fechaDesde.ToString("yyyy-MM-dd");
        var hasta = fechaHasta.ToString("yyyy-MM-dd");

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                              xmlns:tns="Cotiza">
              <soapenv:Body>
                <tns:wsbcucotizaciones.Execute>
                  <tns:Entrada>
                    <tns:Moneda>
                      <tns:item>{codigoMoneda}</tns:item>
                    </tns:Moneda>
                    <tns:FechaDesde>{desde}</tns:FechaDesde>
                    <tns:FechaHasta>{hasta}</tns:FechaHasta>
                    <tns:Grupo>0</tns:Grupo>
                  </tns:Entrada>
                </tns:wsbcucotizaciones.Execute>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    private async Task<XDocument?> PostSoap(string soapEnvelope)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("BCU");
            using var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "Cotizaaction/AWSBCUCOTIZACIONES.Execute");

            using var response = await client.PostAsync(BcuWsdlUrl, content);
            var xml = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BCU respondió {Status}. Cuerpo: {Body}", response.StatusCode, xml);
                return null;
            }

            return XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al conectar con BCU SOAP.");
            return null;
        }
    }

    private List<(DateOnly Fecha, decimal Valor)> ParsearRespuestaBCU(XDocument doc, string moneda)
    {
        var resultados = new List<(DateOnly, decimal)>();

        try
        {
            // Verificar estado de respuesta BCU antes de parsear datos
            var statusEl = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "respuestastatus");
            if (statusEl is not null)
            {
                var codigoError = statusEl.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "codigoerror")?.Value?.Trim();
                var mensaje = statusEl.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "mensaje")?.Value?.Trim();
                if (!string.IsNullOrEmpty(codigoError) && codigoError != "0")
                {
                    _logger.LogWarning(
                        "BCU devolvió error {Codigo} para {Moneda}: {Mensaje}",
                        codigoError, moneda, mensaje);
                    return resultados;
                }
            }

            // La respuesta BCU tiene elementos <datoscotizaciones.dato> con <Fecha> y <TCV>
            // Fecha es xsd:date → yyyy-MM-dd; se intenta también dd/MM/yyyy como fallback
            var items = doc.Descendants()
                .Where(e => e.Name.LocalName == "datoscotizaciones.dato");

            foreach (var item in items)
            {
                var fechaStr = item.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "Fecha")?.Value;
                var tcvStr = item.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "TCV")?.Value;

                if (string.IsNullOrWhiteSpace(fechaStr) || string.IsNullOrWhiteSpace(tcvStr))
                    continue;

                // xsd:date devuelve yyyy-MM-dd; fallback a dd/MM/yyyy
                if (!DateOnly.TryParseExact(fechaStr.Trim(), "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var fecha) &&
                    !DateOnly.TryParseExact(fechaStr.Trim(), "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out fecha))
                    continue;

                if (!decimal.TryParse(tcvStr.Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var tcv))
                    continue;

                if (tcv > 0)
                    resultados.Add((fecha, Math.Round(tcv, 4)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al parsear respuesta BCU para {Moneda}.", moneda);
        }

        return resultados;
    }

    // ── Helper interno ───────────────────────────────────────────────────

    private async Task GuardarSiNoExiste(string moneda, DateOnly fecha, decimal valor, string fuente)
    {
        if (await _repository.ExisteParaMonedaYFecha(moneda, fecha))
            return;

        await _repository.Guardar(new TipoDeCambio
        {
            Id = Guid.NewGuid(),
            Moneda = moneda,
            Fecha = fecha,
            Valor = valor,
            FuenteOrigen = fuente
        });
    }
}
