using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class LibroMayorService : ILibroMayorService
{
    private readonly IAsientoContableRepository _asientoRepository;
    private readonly ICuentaContableRepository _cuentaRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly IEjercicioContableRepository _ejercicioRepository;
    private readonly ILogger<LibroMayorService> _logger;

    public LibroMayorService(
        IAsientoContableRepository asientoRepository,
        ICuentaContableRepository cuentaRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        IEjercicioContableRepository ejercicioRepository,
        ILogger<LibroMayorService> logger)
    {
        _asientoRepository = asientoRepository;
        _cuentaRepository = cuentaRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
        _ejercicioRepository = ejercicioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Genera el Libro Mayor en base a los asientos confirmados y la naturaleza de cada cuenta.
    /// </summary>
    public async Task<LibroMayorResponseDto> Obtener(LibroMayorFiltroDto filtro)
    {
        var sw = Stopwatch.StartNew();

        var plan = await _planDeCuentasRepository.ObtenerPorClienteId(filtro.ClienteId)
            ?? throw new EntidadNoEncontradaException("PlanDeCuentas", filtro.ClienteId);

        if (filtro.EjercicioId.HasValue)
        {
            var ejercicio = await _ejercicioRepository.ObtenerPorId(filtro.EjercicioId.Value)
                ?? throw new EntidadNoEncontradaException("EjercicioContable", filtro.EjercicioId.Value);

            if (ejercicio.ClienteId != filtro.ClienteId)
                throw new AccesoNoAutorizadoException("El ejercicio contable no pertenece al cliente indicado.");
        }

        var cuentas = await _cuentaRepository.ObtenerTodasPorPlan(plan.Id);

        if (filtro.CuentaIds is { Count: > 0 })
        {
            cuentas = cuentas.Where(c => filtro.CuentaIds.Contains(c.Id)).ToList();
        }

        var movimientosPeriodo = await _asientoRepository.ObtenerMovimientosMayor(
            filtro.ClienteId,
            cuentas.Select(c => c.Id),
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.EjercicioId);

        var movimientosIniciales = filtro.FechaDesde.HasValue
            ? await _asientoRepository.ObtenerMovimientosMayor(
                filtro.ClienteId,
                cuentas.Select(c => c.Id),
                null,
                filtro.FechaDesde.Value.AddDays(-1),
                filtro.EjercicioId)
            : new List<LineaAsiento>();

        var cuentasDto = new List<LibroMayorCuentaDto>();

        foreach (var cuenta in cuentas.OrderBy(c => c.Codigo))
        {
            var naturaleza = cuenta.Naturaleza;
            var inicial = movimientosIniciales.Where(m => m.CuentaContableId == cuenta.Id);
            var periodo = movimientosPeriodo.Where(m => m.CuentaContableId == cuenta.Id)
                .OrderBy(m => m.Asiento.Fecha)
                .ThenBy(m => m.Asiento.Numero)
                .ThenBy(m => m.Id)
                .ToList();

            var saldoInicial = CalcularSaldo(naturaleza, inicial);
            var saldoInicialBase = CalcularSaldoBase(naturaleza, inicial);

            var debitos = periodo.Sum(m => m.Debe);
            var creditos = periodo.Sum(m => m.Haber);
            var debitosBase = periodo.Sum(m => m.Debe * m.TipoCambio);
            var creditosBase = periodo.Sum(m => m.Haber * m.TipoCambio);

            var saldoFinal = saldoInicial + CalcularSaldo(naturaleza, periodo);
            var saldoFinalBase = saldoInicialBase + CalcularSaldoBase(naturaleza, periodo);

            var movimientosDto = new List<LibroMayorMovimientoDto>();
            var saldoAcumulado = saldoInicial;
            var saldoAcumuladoBase = saldoInicialBase;

            foreach (var movimiento in periodo)
            {
                var debe = movimiento.Debe;
                var haber = movimiento.Haber;
                var debeBase = debe * movimiento.TipoCambio;
                var haberBase = haber * movimiento.TipoCambio;

                var delta = naturaleza == "Acreedora" ? haber - debe : debe - haber;
                var deltaBase = naturaleza == "Acreedora" ? haberBase - debeBase : debeBase - haberBase;

                saldoAcumulado += delta;
                saldoAcumuladoBase += deltaBase;

                movimientosDto.Add(new LibroMayorMovimientoDto
                {
                    AsientoId = movimiento.AsientoId,
                    NumeroAsiento = movimiento.Asiento.Numero,
                    Fecha = movimiento.Asiento.Fecha,
                    Glosa = movimiento.Asiento.Glosa,
                    Debe = debe,
                    Haber = haber,
                    Moneda = movimiento.Moneda,
                    TipoCambio = movimiento.TipoCambio,
                    DebeBase = debeBase,
                    HaberBase = haberBase,
                    SaldoAcumulado = saldoAcumulado,
                    SaldoAcumuladoBase = saldoAcumuladoBase
                });
            }

            cuentasDto.Add(new LibroMayorCuentaDto
            {
                CuentaId = cuenta.Id,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                Tipo = cuenta.Tipo,
                Naturaleza = cuenta.Naturaleza,
                SaldoInicial = saldoInicial,
                Debitos = debitos,
                Creditos = creditos,
                SaldoFinal = saldoFinal,
                SaldoInicialBase = saldoInicialBase,
                DebitosBase = debitosBase,
                CreditosBase = creditosBase,
                SaldoFinalBase = saldoFinalBase,
                Movimientos = movimientosDto
            });
        }

        var resultado = new LibroMayorResponseDto
        {
            ClienteId = filtro.ClienteId,
            FechaDesde = filtro.FechaDesde,
            FechaHasta = filtro.FechaHasta,
            EjercicioId = filtro.EjercicioId,
            Cuentas = cuentasDto
        };

        sw.Stop();
        if (sw.ElapsedMilliseconds > 2000)
            _logger.LogWarning("Libro Mayor generado con tiempo elevado | ClienteId: {ClienteId} | Cuentas: {Cuentas} | Tiempo: {TiempoMs}ms",
                filtro.ClienteId, cuentasDto.Count, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation("Libro Mayor generado | ClienteId: {ClienteId} | Cuentas: {Cuentas} | Tiempo: {TiempoMs}ms",
                filtro.ClienteId, cuentasDto.Count, sw.ElapsedMilliseconds);

        return resultado;
    }

    /// <summary>
    /// Calcula el saldo en moneda original considerando la naturaleza de la cuenta.
    /// </summary>
    private static decimal CalcularSaldo(string naturaleza, IEnumerable<LineaAsiento> movimientos)
    {
        var debe = movimientos.Sum(m => m.Debe);
        var haber = movimientos.Sum(m => m.Haber);
        return naturaleza == "Acreedora" ? haber - debe : debe - haber;
    }

    /// <summary>
    /// Calcula el saldo en moneda base considerando la naturaleza de la cuenta.
    /// </summary>
    private static decimal CalcularSaldoBase(string naturaleza, IEnumerable<LineaAsiento> movimientos)
    {
        var debe = movimientos.Sum(m => m.Debe * m.TipoCambio);
        var haber = movimientos.Sum(m => m.Haber * m.TipoCambio);
        return naturaleza == "Acreedora" ? haber - debe : debe - haber;
    }
}
