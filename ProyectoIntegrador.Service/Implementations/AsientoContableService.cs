using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class AsientoContableService : IAsientoContableService
{
    private readonly IAsientoContableRepository _asientoRepository;
    private readonly ICuentaContableRepository _cuentaRepository;
    private readonly IEjercicioContableRepository _ejercicioRepository;
    private readonly ISaldoCuentaRepository _saldoRepository;
    private readonly AppDbContext _context;

    public AsientoContableService(
        IAsientoContableRepository asientoRepository,
        ICuentaContableRepository cuentaRepository,
        IEjercicioContableRepository ejercicioRepository,
        ISaldoCuentaRepository saldoRepository,
        AppDbContext context)
    {
        _asientoRepository = asientoRepository;
        _cuentaRepository = cuentaRepository;
        _ejercicioRepository = ejercicioRepository;
        _saldoRepository = saldoRepository;
        _context = context;
    }

    public async Task<AsientoContableDto> Crear(CrearAsientoContableDto dto, Guid usuarioId)
    {
        if (dto.Lineas == null || dto.Lineas.Count < 2)
            throw new ValidacionException("Un asiento debe tener al menos dos líneas.");

        if (dto.Lineas.Select(l => l.CuentaContableId).Distinct().Count() != dto.Lineas.Count)
            throw new ValidacionException("Las líneas del asiento deben pertenecer a cuentas contables distintas.");

        var ejercicio = await _ejercicioRepository.ObtenerPorId(dto.EjercicioId)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", dto.EjercicioId);

        if (ejercicio.Estado == "Cerrado")
            throw new EjercicioCerradoException($"No se puede operar sobre el ejercicio contable del {ejercicio.FechaInicio:dd/MM/yyyy} al {ejercicio.FechaFin:dd/MM/yyyy} porque ya está cerrado.");

        if (ejercicio.ClienteId != dto.ClienteId)
            throw new AccesoNoAutorizadoException("El ejercicio contable no pertenece al cliente indicado.");

        if (dto.Fecha < ejercicio.FechaInicio || dto.Fecha > ejercicio.FechaFin)
            throw new ValidacionException("La fecha del asiento está fuera del rango del ejercicio contable.");

        var totalDebe = dto.Lineas.Sum(l => l.Debe);
        var totalHaber = dto.Lineas.Sum(l => l.Haber);

        if (totalDebe != totalHaber)
            throw new AsientoDesbalanceadoException(totalDebe, totalHaber);

        foreach (var linea in dto.Lineas)
        {
            var cuenta = await _cuentaRepository.ObtenerPorId(linea.CuentaContableId)
                ?? throw new EntidadNoEncontradaException("CuentaContable", linea.CuentaContableId);

            if (!cuenta.EsImputable)
                throw new CuentaNoImputableException(linea.CuentaContableId, cuenta.Codigo);

            if (cuenta.Estado != "Activa")
                throw new ValidacionException($"La cuenta '{cuenta.Codigo}' no está activa.");
        }

        AsientoContable asiento;

        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var ultimoNumero = await _asientoRepository.ObtenerUltimoNumero(dto.ClienteId, dto.EjercicioId);

            asiento = new AsientoContable
            {
                Id = Guid.NewGuid(),
                ClienteId = dto.ClienteId,
                UsuarioId = usuarioId,
                EjercicioId = dto.EjercicioId,
                Numero = ultimoNumero + 1,
                Fecha = dto.Fecha,
                Glosa = dto.Glosa,
                Estado = "Confirmado",
                LineasAsiento = dto.Lineas.Select(l => new LineaAsiento
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = l.CuentaContableId,
                    CentroCostoId = l.CentroCostoId,
                    Debe = l.Debe,
                    Haber = l.Haber,
                    Moneda = l.Moneda,
                    TipoCambio = l.TipoCambio,
                    ImporteMonedaBase = (l.Debe + l.Haber) * l.TipoCambio
                }).ToList()
            };

            await _asientoRepository.Guardar(asiento);

            // Actualización transaccional de saldos pre-calculados
            await ActualizarSaldos(asiento.LineasAsiento, dto.ClienteId, dto.EjercicioId, dto.Fecha);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await ObtenerPorId(asiento.Id);
    }

    public async Task<AsientoContableDto> ObtenerPorId(Guid id)
    {
        var asiento = await _asientoRepository.ObtenerPorIdConLineas(id)
            ?? throw new EntidadNoEncontradaException("AsientoContable", id);

        return MapearADto(asiento);
    }

    public async Task<(List<AsientoContableResumenDto> Items, int Total)> Listar(FiltroAsientoDto filtro)
    {
        List<AsientoContable> items;
        int total;

        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue)
        {
            items = await _asientoRepository.ObtenerPorRangoFecha(
                filtro.ClienteId, filtro.FechaDesde.Value, filtro.FechaHasta.Value,
                filtro.Pagina, filtro.CantidadPorPagina);

            total = await _asientoRepository.ContarPorRangoFecha(
                filtro.ClienteId, filtro.FechaDesde.Value, filtro.FechaHasta.Value);
        }
        else if (filtro.EjercicioId.HasValue)
        {
            items = await _asientoRepository.ObtenerPorEjercicio(
                filtro.ClienteId, filtro.EjercicioId.Value,
                filtro.Pagina, filtro.CantidadPorPagina);

            total = await _asientoRepository.ContarPorEjercicio(filtro.ClienteId, filtro.EjercicioId.Value);
        }
        else
        {
            items = await _asientoRepository.ObtenerPorCliente(
                filtro.ClienteId, filtro.Pagina, filtro.CantidadPorPagina);

            total = await _asientoRepository.ContarPorCliente(filtro.ClienteId);
        }

        var resumenes = items.Select(a => new AsientoContableResumenDto
        {
            Id = a.Id,
            EjercicioId = a.EjercicioId,
            Numero = a.Numero,
            Fecha = a.Fecha,
            Glosa = a.Glosa,
            Estado = a.Estado,
            TotalDebe = a.LineasAsiento.Sum(l => l.Debe),
            TotalHaber = a.LineasAsiento.Sum(l => l.Haber)
        }).ToList();

        return (resumenes, total);
    }

    public async Task<AsientoContableDto> Revertir(Guid asientoId, Guid usuarioId)
    {
        var original = await _asientoRepository.ObtenerPorIdConLineas(asientoId)
            ?? throw new EntidadNoEncontradaException("AsientoContable", asientoId);

        if (original.Estado == "Revertido")
            throw new AsientoYaRevertidoException(asientoId);

        AsientoContable asientoInverso;

        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var ultimoNumero = await _asientoRepository.ObtenerUltimoNumero(original.ClienteId, original.EjercicioId);

            asientoInverso = new AsientoContable
            {
                Id = Guid.NewGuid(),
                ClienteId = original.ClienteId,
                UsuarioId = usuarioId,
                EjercicioId = original.EjercicioId,
                AsientoOrigenId = original.Id,
                Numero = ultimoNumero + 1,
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Glosa = $"Reversión del asiento N° {original.Numero}: {original.Glosa}",
                Estado = "Confirmado",
                LineasAsiento = original.LineasAsiento.Select(l => new LineaAsiento
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = l.CuentaContableId,
                    CentroCostoId = l.CentroCostoId,
                    Debe = l.Haber,
                    Haber = l.Debe,
                    Moneda = l.Moneda,
                    TipoCambio = l.TipoCambio,
                    ImporteMonedaBase = l.ImporteMonedaBase
                }).ToList()
            };

            original.Estado = "Revertido";

            await _asientoRepository.Guardar(asientoInverso);
            await _asientoRepository.Actualizar(original);

            // Actualización transaccional de saldos del asiento inverso
            await ActualizarSaldos(
                asientoInverso.LineasAsiento,
                original.ClienteId,
                original.EjercicioId,
                asientoInverso.Fecha);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await ObtenerPorId(asientoInverso.Id);
    }

    // ──────────────────────────────────────────────
    // Saldos pre-calculados
    // ──────────────────────────────────────────────

    private async Task ActualizarSaldos(
        ICollection<LineaAsiento> lineas,
        Guid clienteId,
        Guid ejercicioId,
        DateOnly fecha)
    {
        // El período es el primer día del mes de la fecha del asiento
        var periodo = new DateOnly(fecha.Year, fecha.Month, 1);

        foreach (var linea in lineas)
        {
            var saldo = await _saldoRepository.ObtenerPorPeriodo(
                clienteId, linea.CuentaContableId, ejercicioId, periodo);

            if (saldo is null)
            {
                saldo = new SaldoCuenta
                {
                    Id = Guid.NewGuid(),
                    ClienteId = clienteId,
                    CuentaContableId = linea.CuentaContableId,
                    EjercicioId = ejercicioId,
                    Periodo = periodo,
                    DebeAcumulado = linea.Debe,
                    HaberAcumulado = linea.Haber,
                    Saldo = linea.Debe - linea.Haber
                };

                await _saldoRepository.Guardar(saldo);
            }
            else
            {
                saldo.DebeAcumulado += linea.Debe;
                saldo.HaberAcumulado += linea.Haber;
                saldo.Saldo = saldo.DebeAcumulado - saldo.HaberAcumulado;

                await _saldoRepository.Actualizar(saldo);
            }
        }
    }

    // ──────────────────────────────────────────────
    // Helpers privados
    // ──────────────────────────────────────────────

    private static AsientoContableDto MapearADto(AsientoContable a)
    {
        var lineas = a.LineasAsiento.Select(l => new LineaAsientoDto
        {
            Id = l.Id,
            CuentaContableId = l.CuentaContableId,
            CodigoCuenta = l.CuentaContable?.Codigo ?? string.Empty,
            NombreCuenta = l.CuentaContable?.Nombre ?? string.Empty,
            CentroCostoId = l.CentroCostoId,
            Debe = l.Debe,
            Haber = l.Haber,
            Moneda = l.Moneda,
            TipoCambio = l.TipoCambio,
            ImporteMonedaBase = l.ImporteMonedaBase
        }).ToList();

        return new AsientoContableDto
        {
            Id = a.Id,
            Numero = a.Numero,
            Fecha = a.Fecha,
            Glosa = a.Glosa,
            Estado = a.Estado,
            ClienteId = a.ClienteId,
            EjercicioId = a.EjercicioId,
            UsuarioId = a.UsuarioId,
            AsientoOrigenId = a.AsientoOrigenId,
            Lineas = lineas,
            TotalDebe = lineas.Sum(l => l.Debe),
            TotalHaber = lineas.Sum(l => l.Haber)
        };
    }
}