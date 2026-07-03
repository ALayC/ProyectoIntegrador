using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;
using System.Diagnostics;

namespace ProyectoIntegrador.Service.Implementations
{
    public class BalanceGeneralService : IBalanceGeneralService
    {
        private readonly ILineaAsientoRepository _lineaAsientoRepository;
        private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
        private readonly ICuentaContableRepository _cuentaContableRepository;
        private readonly ILogger<BalanceGeneralService> _logger;

        public BalanceGeneralService(
            ILineaAsientoRepository lineaAsientoRepository,
            IPlanDeCuentasRepository planDeCuentasRepository,
            ICuentaContableRepository cuentaContableRepository,
            ILogger<BalanceGeneralService> logger)
        {
            _lineaAsientoRepository = lineaAsientoRepository;
            _planDeCuentasRepository = planDeCuentasRepository;
            _cuentaContableRepository = cuentaContableRepository;
            _logger = logger;
        }

        public async Task<BalanceGeneralResponseDto> Generar(BalanceGeneralFiltroDto filtro)
        {
            var sw = Stopwatch.StartNew();

            var plan = await _planDeCuentasRepository
            .ObtenerPorClienteId(filtro.ClienteId) ?? throw new EntidadNoEncontradaException("PlanDeCuentas", filtro.ClienteId);

            var cuentas = await _cuentaContableRepository.ObtenerTodasPorPlan(plan.Id);

            var lineas = await _lineaAsientoRepository.ObtenerParaBalanceGeneral(filtro.ClienteId, filtro.FechaHasta);

            var nodos = cuentas.ToDictionary(
            c => c.Id,
            c => new BalanceGeneralNodoDto
            {
                CuentaId = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Saldo = 0
            });

            var cuentasPorId = cuentas.ToDictionary(c => c.Id);

            foreach (var linea in lineas)
            {
                if (!cuentasPorId.TryGetValue(linea.CuentaContableId, out var cuenta))
                {
                    continue;
                }
                nodos[cuenta.Id].Saldo += CalcularSaldoMovimiento(linea, cuenta.Naturaleza);
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

            var activos = cuentas
            .Where(c => c.CuentaPadreId == null && c.Tipo == "Activo")
            .Select(c => nodos[c.Id])
            .ToList();

            var pasivos = cuentas
            .Where(c => c.CuentaPadreId == null && c.Tipo == "Pasivo")
            .Select(c => nodos[c.Id])
            .ToList();

            var patrimonio = cuentas
            .Where(c => c.CuentaPadreId == null && c.Tipo == "Patrimonio")
            .Select(c => nodos[c.Id])
            .ToList();

            foreach (var activo in activos)
            {
                AcumularSaldos(activo);
            }
            foreach (var pasivo in pasivos)
            {
                AcumularSaldos(pasivo);
            }
            foreach (var patrimonioNodo in patrimonio)
            {
                AcumularSaldos(patrimonioNodo);
            }

            activos = activos
            .Where(RemoverRamasVacias)
            .ToList();

            pasivos = pasivos
                .Where(RemoverRamasVacias)
                .ToList();

            patrimonio = patrimonio
                .Where(RemoverRamasVacias)
                .ToList();

            var totalActivos = activos.Sum(a => a.Saldo);

            var totalPasivos = pasivos.Sum(p => p.Saldo);

            var totalPatrimonio = patrimonio.Sum(p => p.Saldo);

            var totalPasivoPatrimonio = totalPasivos + totalPatrimonio;

            var balancea = totalActivos == totalPasivos + totalPatrimonio;

            var resultado = new BalanceGeneralResponseDto
            {
                TotalActivo = totalActivos,
                TotalPasivo = totalPasivos,
                TotalPatrimonio = totalPatrimonio,
                TotalPasivoPatrimonio = totalPasivoPatrimonio,
                Balancea = balancea,
                Activos = activos,
                Pasivos = pasivos,
                Patrimonio = patrimonio
            };

            sw.Stop();
            if (sw.ElapsedMilliseconds > 2000)
                _logger.LogWarning("Estado de Resultados generado con tiempo elevado | ClienteId: {ClienteId} | Tiempo: {TiempoMs}ms",
                    filtro.ClienteId, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation("Estado de Resultados generado | ClienteId: {ClienteId} | Tiempo: {TiempoMs}ms",
                    filtro.ClienteId, sw.ElapsedMilliseconds);

            return resultado;
        }

        private static decimal CalcularSaldoMovimiento(LineaAsiento linea, string naturaleza)
        {
            var debeBase  = linea.Debe  * linea.TipoCambio;
            var haberBase = linea.Haber * linea.TipoCambio;
            return naturaleza == "Acreedora"
                ? haberBase - debeBase
                : debeBase  - haberBase;
        }

        private static decimal AcumularSaldos(BalanceGeneralNodoDto nodo)
        {
            foreach (var hija in nodo.Hijas)
            {
                nodo.Saldo += AcumularSaldos(hija);
            }
            return nodo.Saldo;
        }

        private static bool RemoverRamasVacias(BalanceGeneralNodoDto nodo)
        {
            nodo.Hijas = nodo.Hijas
                .Where(RemoverRamasVacias)
                .ToList();

            return nodo.Saldo != 0 || nodo.Hijas.Any();
        }
    }
}
