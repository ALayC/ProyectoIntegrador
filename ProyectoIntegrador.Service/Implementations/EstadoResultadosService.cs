using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class EstadoResultadosService : IEstadoResultadosService
{
    private readonly ILineaAsientoRepository _lineaAsientoRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly ICuentaContableRepository _cuentaContableRepository;
    private readonly ILogger<EstadoResultadosService> _logger;

    public EstadoResultadosService(
        ILineaAsientoRepository lineaAsientoRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        ICuentaContableRepository cuentaContableRepository,
        ILogger<EstadoResultadosService> logger)
    {
        _lineaAsientoRepository = lineaAsientoRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
        _cuentaContableRepository = cuentaContableRepository;
        _logger = logger;
    }

    public async Task<EstadoResultadosResponseDto> Generar(EstadoResultadosFiltroDto filtro)
    {
        var sw = Stopwatch.StartNew();

        if (filtro.FechaDesde > filtro.FechaHasta)
        {
            throw new ValidacionException("La fecha desde no puede ser mayor a la fecha hasta.");
        }
        var plan = await _planDeCuentasRepository
        .ObtenerPorClienteId(filtro.ClienteId) ?? throw new EntidadNoEncontradaException("PlanDeCuentas", filtro.ClienteId);

        var cuentas = await _cuentaContableRepository.ObtenerTodasPorPlan(plan.Id);

        var lineas = await _lineaAsientoRepository.ObtenerParaEstadoResultados(filtro.ClienteId, filtro.FechaDesde, filtro.FechaHasta);

        var nodos = cuentas.ToDictionary(
        c => c.Id,
        c => new EstadoResultadoNodoDto
        {
            CuentaId = c.Id,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            Importe = 0
        });

        var cuentasPorId = cuentas.ToDictionary(c => c.Id);

        foreach (var linea in lineas)
        {
            if (!cuentasPorId.TryGetValue(linea.CuentaContableId, out var cuenta))
            {
                continue;
            }
            decimal importe = 0;
            if (cuenta.Naturaleza == "Acreedora")
            {
                importe = linea.Haber - linea.Debe;
            }
            else if (cuenta.Naturaleza == "Deudora")
            {
                importe = linea.Debe - linea.Haber;
            }
            else
            {
                continue;
            }
            nodos[cuenta.Id].Importe += importe;
        }

        foreach (var cuenta in cuentas)
        {
            if (!cuenta.CuentaPadreId.HasValue)
            {
                continue;
            }
            if (!nodos.TryGetValue(cuenta.CuentaPadreId.Value, out var padre))
            {
                continue;
            }
            padre.Hijas.Add(nodos[cuenta.Id]);
        }

        var ingresos = cuentas
        .Where(c => c.CuentaPadreId == null && c.Tipo == "Ingreso")
        .Select(c => nodos[c.Id])
        .ToList();

        var egresos = cuentas
        .Where(c => c.CuentaPadreId == null && c.Tipo == "Egreso")
        .Select(c => nodos[c.Id])
        .ToList();

        foreach (var ingreso in ingresos)
        {
            AcumularImportes(ingreso);
        }
        foreach (var egreso in egresos)
        {
            AcumularImportes(egreso);
        }

        ingresos = ingresos
        .Where(RemoverRamasVacias)
        .ToList();

        egresos = egresos
        .Where(RemoverRamasVacias)
        .ToList();

        var totalIngresos = ingresos.Sum(i => i.Importe);

        var totalEgresos = egresos.Sum(e => e.Importe);

        var resultadoNeto = totalIngresos - totalEgresos;

        sw.Stop();
        if (sw.ElapsedMilliseconds > 2000)
            _logger.LogWarning("Estado de Resultados generado con tiempo elevado | ClienteId: {ClienteId} | Tiempo: {TiempoMs}ms",
                filtro.ClienteId, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation("Estado de Resultados generado | ClienteId: {ClienteId} | Tiempo: {TiempoMs}ms",
                filtro.ClienteId, sw.ElapsedMilliseconds);

        return new EstadoResultadosResponseDto
        {
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            ResultadoNeto = resultadoNeto,
            Ingresos = ingresos,
            Egresos = egresos
        };
    }

    private static decimal AcumularImportes(EstadoResultadoNodoDto nodo)
    {
        foreach (var hija in nodo.Hijas)
        {
            nodo.Importe += AcumularImportes(hija);
        }
        return nodo.Importe;
    }

    private static bool RemoverRamasVacias(EstadoResultadoNodoDto nodo)
    {
        nodo.Hijas = nodo.Hijas
            .Where(hija => RemoverRamasVacias(hija))
            .ToList();
        return nodo.Importe != 0 || nodo.Hijas.Any();
    }
}

