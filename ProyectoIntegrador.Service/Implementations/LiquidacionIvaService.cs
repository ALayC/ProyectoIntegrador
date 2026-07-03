using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class LiquidacionIvaService : ILiquidacionIvaService
{
    private readonly ILineaAsientoRepository _lineaAsientoRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly ILogger<LiquidacionIvaService> _logger;

    public LiquidacionIvaService(
        ILineaAsientoRepository lineaAsientoRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        ILogger<LiquidacionIvaService> logger)
    {
        _lineaAsientoRepository = lineaAsientoRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
        _logger = logger;
    }

    public async Task<LiquidacionIvaResponseDto> Calcular(LiquidacionIvaFiltroDto filtro)
    {
        if (filtro.Mes < 1 || filtro.Mes > 12)
            throw new ValidacionException("El mes debe estar entre 1 y 12.");

        if (filtro.Anio < 2000 || filtro.Anio > 2100)
            throw new ValidacionException("El año indicado no es válido.");

        var sw = Stopwatch.StartNew();

        _ = await _planDeCuentasRepository.ObtenerPorClienteId(filtro.ClienteId)
            ?? throw new EntidadNoEncontradaException("PlanDeCuentas", filtro.ClienteId);

        var fechaDesde = new DateOnly(filtro.Anio, filtro.Mes, 1);
        var fechaHasta = fechaDesde.AddMonths(1).AddDays(-1);

        var lineas = await _lineaAsientoRepository
            .ObtenerParaLiquidacionIva(filtro.ClienteId, fechaDesde, fechaHasta);

        decimal totalIvaVentas = 0;
        decimal totalIvaCompras = 0;

        foreach (var linea in lineas)
        {
            var nombre = linea.CuentaContable.Nombre;

            if (nombre == "IVA Débito Fiscal")
            {
                // Cuenta acreedora: saldo = Haber - Debe, convertido a moneda base
                totalIvaVentas += (linea.Haber - linea.Debe) * linea.TipoCambio;
            }
            else if (nombre == "IVA Crédito Fiscal")
            {
                // Cuenta deudora: saldo = Debe - Haber, convertido a moneda base
                totalIvaCompras += (linea.Debe - linea.Haber) * linea.TipoCambio;
            }
        }

        var saldoNeto = totalIvaVentas - totalIvaCompras;

        sw.Stop();
        _logger.LogInformation(
            "Liquidación IVA calculada | ClienteId: {ClienteId} | Período: {Mes}/{Anio} | Tiempo: {TiempoMs}ms",
            filtro.ClienteId, filtro.Mes, filtro.Anio, sw.ElapsedMilliseconds);

        return new LiquidacionIvaResponseDto
        {
            Mes = filtro.Mes,
            Anio = filtro.Anio,
            TotalIvaVentas = totalIvaVentas,
            TotalIvaCompras = totalIvaCompras,
            SaldoNeto = Math.Abs(saldoNeto),
            TipoSaldo = saldoNeto > 0 ? "APagar" : "AFavor"
        };
    }
}
