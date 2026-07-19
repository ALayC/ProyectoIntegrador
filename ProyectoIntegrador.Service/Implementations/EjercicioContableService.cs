using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Constants;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class EjercicioContableService : IEjercicioContableService
{
    private readonly IEjercicioContableRepository _ejercicioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPlanDeCuentasRepository _planRepository;
    private readonly IAsientoContableRepository _asientoRepository;
    private readonly ISaldoCuentaRepository _saldoRepository;
    private readonly AppDbContext _context;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<EjercicioContableService> _logger;

    public EjercicioContableService(
        IEjercicioContableRepository ejercicioRepository,
        IClienteRepository clienteRepository,
        IPlanDeCuentasRepository planRepository,
        IAsientoContableRepository asientoRepository,
        ISaldoCuentaRepository saldoRepository,
        AppDbContext context,
        IAuditoriaService auditoriaService,
        ILogger<EjercicioContableService> logger)
    {
        _ejercicioRepository = ejercicioRepository;
        _clienteRepository = clienteRepository;
        _planRepository = planRepository;
        _asientoRepository = asientoRepository;
        _saldoRepository = saldoRepository;
        _context = context;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    public async Task<EjercicioContableResponseDto> Crear(CrearEjercicioContableDto dto)
    {
        if (!dto.ClienteId.HasValue || !dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
        {
            throw new ValidacionException("El cliente y las fechas son obligatorios.");
        }

        var clienteId = dto.ClienteId.Value;
        var fechaInicio = dto.FechaInicio.Value;
        var fechaFin = dto.FechaFin.Value;

        await ValidarClienteExistente(clienteId);
        ValidarRangoFechas(fechaInicio, fechaFin);

        var ejercicio = new EjercicioContable
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Estado = "Abierto"
        };

        await _ejercicioRepository.Guardar(ejercicio);

        await _auditoriaService.Registrar(
            dto.UsuarioId,
            AuditoriaConstantes.Entidades.EjercicioContable,
            AuditoriaConstantes.Acciones.Crear,
            datosAnteriores: null,
            datosNuevos: ConstruirDatosAuditoria(ejercicio));

        _logger.LogInformation("Ejercicio contable creado | Id: {EjercicioId} | ClienteId: {ClienteId} | Desde: {FechaInicio} | Hasta: {FechaFin}",
            ejercicio.Id, clienteId, fechaInicio, fechaFin);

        return Mapear(ejercicio);
    }

    public async Task<EjercicioContableResponseDto> ObtenerPorId(Guid id)
    {
        var ejercicio = await _ejercicioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", id);

        return Mapear(ejercicio);
    }

    public async Task<PaginadoDto<EjercicioContableResponseDto>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina)
    {
        if (pagina < 1 || cantidadPorPagina <= 0)
        {
            throw new ValidacionException("Los parámetros de paginación no son válidos.");
        }

        await ValidarClienteExistente(clienteId);

        var ejercicios = await _ejercicioRepository.ObtenerPorCliente(clienteId, pagina, cantidadPorPagina);
        var total = await _ejercicioRepository.ContarPorCliente(clienteId);

        var ejerciciosDto = ejercicios.Select(Mapear).ToList();
        return new PaginadoDto<EjercicioContableResponseDto>(ejerciciosDto, pagina, cantidadPorPagina, total);
    }

    public async Task<EjercicioContableResponseDto> Actualizar(Guid id, ActualizarEjercicioContableDto dto)
    {
        var ejercicio = await _ejercicioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", id);

        if (ejercicio.Estado == "Cerrado")
        {
            throw new EjercicioCerradoException(id);
        }

        if (!dto.FechaInicio.HasValue || !dto.FechaFin.HasValue)
        {
            throw new ValidacionException("La fecha de inicio y fin son obligatorias.");
        }

        var fechaInicio = dto.FechaInicio.Value;
        var fechaFin = dto.FechaFin.Value;

        ValidarRangoFechas(fechaInicio, fechaFin);

        ejercicio.FechaInicio = fechaInicio;
        ejercicio.FechaFin = fechaFin;

        await _ejercicioRepository.Actualizar(ejercicio);
        return Mapear(ejercicio);
    }

    public async Task<CierreEjercicioResponseDto> Cerrar(Guid id, Guid usuarioId)
    {
        var ejercicio = await _ejercicioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", id);

        if (ejercicio.Estado == "Cerrado")
            throw new EjercicioCerradoException(id);

        var plan = await _planRepository.ObtenerPorClienteId(ejercicio.ClienteId)
            ?? throw new EntidadNoEncontradaException("PlanDeCuentas", ejercicio.ClienteId);

        // ?? Buscar o crear las cuentas de cierre ??????????????????????????????????????
        var cuentaResumen = await ObtenerOCrearCuentaCierre(
            plan.Id, "9901", "Resumen de resultados", "Patrimonio", "Acreedora");

        var cuentaResultadoEjercicio = await ObtenerOCrearCuentaCierre(
            plan.Id, "9902", "Resultado del ejercicio", "Patrimonio", "Acreedora");

        var cuentaResultadosAcumulados = await ObtenerOCrearCuentaCierre(
            plan.Id, "9903", "Resultados acumulados", "Patrimonio", "Acreedora");

        // ?? Calcular saldos por cuenta del ejercicio (en moneda base) ?????
        var lineasEjercicio = await _context.LineasAsiento
            .Include(l => l.Asiento)
            .Include(l => l.CuentaContable)
            .Where(l =>
                l.Asiento.EjercicioId == ejercicio.Id &&
                l.Asiento.Estado == "Confirmado" &&
                (l.CuentaContable.Tipo == "Ingreso" || l.CuentaContable.Tipo == "Egreso"))
            .ToListAsync();

        // Agrupar saldos netos por cuenta (Debe - Haber en moneda base)
        var saldosPorCuenta = lineasEjercicio
            .GroupBy(l => l.CuentaContable)
            .Select(g => new
            {
                Cuenta = g.Key,
                SaldoNeto = g.Sum(l => l.Debe) - g.Sum(l => l.Haber)
            })
            .ToList();

        var cuentasIngreso = saldosPorCuenta.Where(s => s.Cuenta.Tipo == "Ingreso" && s.SaldoNeto != 0).ToList();
        var cuentasEgreso  = saldosPorCuenta.Where(s => s.Cuenta.Tipo == "Egreso"  && s.SaldoNeto != 0).ToList();

        // Ingresos: naturaleza Acreedora ? saldo normal = Haber > Debe ? SaldoNeto < 0 (Debe-Haber)
        // Para cerrar: debitamos la cuenta de ingreso (por su saldo acreedor) y acreditamos Resumen
        decimal totalIngresos = cuentasIngreso.Sum(s => Math.Abs(s.SaldoNeto));
        // Egresos: naturaleza Deudora ? saldo normal = Debe > Haber ? SaldoNeto > 0
        // Para cerrar: acreditamos la cuenta de egreso (por su saldo deudor) y debitamos Resumen
        decimal totalEgresos  = cuentasEgreso.Sum(s => Math.Abs(s.SaldoNeto));

        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var fechaCierre = ejercicio.FechaFin;
            int asientosGenerados = 0;

            // ?? Asiento 1: Cerrar Ingresos ? Resumen de Resultados ????????
            if (cuentasIngreso.Count > 0)
            {
                var lineasAsiento1 = new List<LineaAsiento>();

                foreach (var s in cuentasIngreso)
                {
                    // Las cuentas de Ingreso (naturaleza Acreedora) tienen saldo en Haber.
                    // Para saldar: Debe en la cuenta de ingreso (igual al monto acreedor = -SaldoNeto)
                    var monto = Math.Abs(s.SaldoNeto);
                    lineasAsiento1.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = s.Cuenta.Id,
                        Debe = monto,
                        Haber = 0,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = monto
                    });
                }

                // Contrapartida: Haber en Resumen de Resultados
                lineasAsiento1.Add(new LineaAsiento
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuentaResumen.Id,
                    Debe = 0,
                    Haber = totalIngresos,
                    Moneda = "UYU",
                    TipoCambio = 1,
                    ImporteMonedaBase = totalIngresos
                });

                var numero1 = await _asientoRepository.ObtenerUltimoNumero(ejercicio.ClienteId, ejercicio.Id);
                var asiento1 = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    ClienteId = ejercicio.ClienteId,
                    UsuarioId = usuarioId,
                    EjercicioId = ejercicio.Id,
                    Numero = numero1 + 1,
                    Fecha = fechaCierre,
                    Glosa = "Cierre de ejercicio – Cancelación de ingresos",
                    Estado = "Confirmado",
                    EsTipoCierre = true,
                    LineasAsiento = lineasAsiento1
                };

                await _asientoRepository.Guardar(asiento1);
                await ActualizarSaldosCierre(asiento1.LineasAsiento, ejercicio.ClienteId, ejercicio.Id, fechaCierre);
                asientosGenerados++;
            }

            // ?? Asiento 2: Cerrar Egresos ? Resumen de Resultados ?????????
            if (cuentasEgreso.Count > 0)
            {
                var lineasAsiento2 = new List<LineaAsiento>();

                // Contrapartida: Debe en Resumen de Resultados
                lineasAsiento2.Add(new LineaAsiento
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = cuentaResumen.Id,
                    Debe = totalEgresos,
                    Haber = 0,
                    Moneda = "UYU",
                    TipoCambio = 1,
                    ImporteMonedaBase = totalEgresos
                });

                foreach (var s in cuentasEgreso)
                {
                    // Las cuentas de Egreso (naturaleza Deudora) tienen saldo en Debe.
                    // Para saldar: Haber en la cuenta de egreso (igual al saldo deudor = SaldoNeto)
                    var monto = Math.Abs(s.SaldoNeto);
                    lineasAsiento2.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = s.Cuenta.Id,
                        Debe = 0,
                        Haber = monto,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = monto
                    });
                }

                var numero2 = await _asientoRepository.ObtenerUltimoNumero(ejercicio.ClienteId, ejercicio.Id);
                var asiento2 = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    ClienteId = ejercicio.ClienteId,
                    UsuarioId = usuarioId,
                    EjercicioId = ejercicio.Id,
                    Numero = numero2 + 1,
                    Fecha = fechaCierre,
                    Glosa = "Cierre de ejercicio – Cancelación de egresos",
                    Estado = "Confirmado",
                    EsTipoCierre = true,
                    LineasAsiento = lineasAsiento2
                };

                await _asientoRepository.Guardar(asiento2);
                await ActualizarSaldosCierre(asiento2.LineasAsiento, ejercicio.ClienteId, ejercicio.Id, fechaCierre);
                asientosGenerados++;
            }

            // ?? Asiento 3: Cerrar Resumen de Resultados ? Resultado del Ejercicio ??
            decimal resultadoNeto = totalIngresos - totalEgresos;

            if (resultadoNeto != 0 || (cuentasIngreso.Count > 0 || cuentasEgreso.Count > 0))
            {
                var lineasAsiento3 = new List<LineaAsiento>();

                if (resultadoNeto > 0)
                {
                    // Ganancia: Debe en Resumen de Resultados, Haber en Resultado del Ejercicio
                    lineasAsiento3.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = cuentaResumen.Id,
                        Debe = resultadoNeto,
                        Haber = 0,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = resultadoNeto
                    });
                    lineasAsiento3.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = cuentaResultadoEjercicio.Id,
                        Debe = 0,
                        Haber = resultadoNeto,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = resultadoNeto
                    });
                }
                else if (resultadoNeto < 0)
                {
                    // Pérdida: Debe en Resultado del Ejercicio, Haber en Resumen de Resultados
                    var perdida = Math.Abs(resultadoNeto);
                    lineasAsiento3.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = cuentaResultadoEjercicio.Id,
                        Debe = perdida,
                        Haber = 0,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = perdida
                    });
                    lineasAsiento3.Add(new LineaAsiento
                    {
                        Id = Guid.NewGuid(),
                        CuentaContableId = cuentaResumen.Id,
                        Debe = 0,
                        Haber = perdida,
                        Moneda = "UYU",
                        TipoCambio = 1,
                        ImporteMonedaBase = perdida
                    });
                }
                else
                {
                    // Resultado neto cero: igual registramos el asiento de cierre con montos cero no tiene sentido.
                    // Solo generamos si hay movimiento real.
                    goto skipAsiento3;
                }

                var numero3 = await _asientoRepository.ObtenerUltimoNumero(ejercicio.ClienteId, ejercicio.Id);
                var asiento3 = new AsientoContable
                {
                    Id = Guid.NewGuid(),
                    ClienteId = ejercicio.ClienteId,
                    UsuarioId = usuarioId,
                    EjercicioId = ejercicio.Id,
                    Numero = numero3 + 1,
                    Fecha = fechaCierre,
                    Glosa = resultadoNeto > 0
                        ? "Cierre de ejercicio – Ganancia del período"
                        : "Cierre de ejercicio – Pérdida del período",
                    Estado = "Confirmado",
                    EsTipoCierre = true,
                    LineasAsiento = lineasAsiento3
                };

                await _asientoRepository.Guardar(asiento3);
                await ActualizarSaldosCierre(asiento3.LineasAsiento, ejercicio.ClienteId, ejercicio.Id, fechaCierre);
                asientosGenerados++;
            }

            skipAsiento3:

            // ?? Marcar ejercicio como cerrado ?????????????????????????????
            var datosAnteriores = ConstruirDatosAuditoria(ejercicio);
            ejercicio.Estado = "Cerrado";
            await _ejercicioRepository.Actualizar(ejercicio);

            await _auditoriaService.Registrar(
                usuarioId,
                AuditoriaConstantes.Entidades.EjercicioContable,
                AuditoriaConstantes.Acciones.Cerrar,
                datosAnteriores: datosAnteriores,
                datosNuevos: ConstruirDatosAuditoria(ejercicio));

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Cierre de ejercicio completado | Id: {EjercicioId} | Ingresos: {Ingresos} | Egresos: {Egresos} | Resultado: {Resultado} | Asientos: {Asientos} | UsuarioId: {UsuarioId}",
                ejercicio.Id, totalIngresos, totalEgresos, resultadoNeto, asientosGenerados, usuarioId);

            return new CierreEjercicioResponseDto
            {
                EjercicioId = ejercicio.Id,
                TotalIngresos = totalIngresos,
                TotalEgresos = totalEgresos,
                ResultadoNeto = resultadoNeto,
                AsientosGenerados = asientosGenerados,
                AsientosCierre = await ObtenerAsientosCierre(ejercicio.Id)
            };
        }
        catch (Exception ex) when (ex is not EjercicioCerradoException
                                     && ex is not EntidadNoEncontradaException
                                     && ex is not ValidacionException)
        {
            _logger.LogError(ex, "Error inesperado al cerrar ejercicio | EjercicioId: {EjercicioId} | UsuarioId: {UsuarioId}", id, usuarioId);
            await transaction.RollbackAsync();
            throw;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ??????????????????????????????????????????????
    // Saldos pre-calculados para asientos de cierre
    // ??????????????????????????????????????????????

    private async Task ActualizarSaldosCierre(
        ICollection<LineaAsiento> lineas,
        Guid clienteId,
        Guid ejercicioId,
        DateOnly fecha)
    {
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
                    Saldo = linea.Debe - linea.Haber,
                    Moneda = linea.Moneda,
                    DebeAcumuladoBase = linea.Debe,
                    HaberAcumuladoBase = linea.Haber,
                    SaldoBase = linea.Debe - linea.Haber
                };
                await _saldoRepository.Guardar(saldo);
            }
            else
            {
                saldo.DebeAcumulado += linea.Debe;
                saldo.HaberAcumulado += linea.Haber;
                saldo.Saldo = saldo.DebeAcumulado - saldo.HaberAcumulado;
                saldo.DebeAcumuladoBase += linea.Debe;
                saldo.HaberAcumuladoBase += linea.Haber;
                saldo.SaldoBase = saldo.DebeAcumuladoBase - saldo.HaberAcumuladoBase;
                await _saldoRepository.Actualizar(saldo);
            }
        }
    }

    // ??????????????????????????????????????????????
    // Métodos privados
    // ??????????????????????????????????????????????

    private static EjercicioContableResponseDto Mapear(EjercicioContable ejercicio) => new()
    {
        Id = ejercicio.Id,
        ClienteId = ejercicio.ClienteId,
        FechaInicio = ejercicio.FechaInicio,
        FechaFin = ejercicio.FechaFin,
        Estado = ejercicio.Estado
    };

    private static void ValidarRangoFechas(DateOnly fechaInicio, DateOnly fechaFin)
    {
        if (fechaInicio >= fechaFin)
        {
            throw new ValidacionException("La fecha de inicio debe ser anterior a la fecha de fin.");
        }
    }

    private static object ConstruirDatosAuditoria(EjercicioContable ejercicio)
    {
        return new
        {
            ejercicio.Id,
            ejercicio.ClienteId,
            ejercicio.FechaInicio,
            ejercicio.FechaFin,
            ejercicio.Estado
        };
    }

    private async Task ValidarClienteExistente(Guid clienteId)
    {
        if (await _clienteRepository.ObtenerPorId(clienteId) is null)
        {
            throw new EntidadNoEncontradaException("Cliente", clienteId);
        }
    }

    public async Task<List<AsientoContableDto>> ObtenerAsientosCierre(Guid ejercicioId)
    {
        var asientos = await _context.AsientosContables
            .Include(a => a.LineasAsiento)
                .ThenInclude(l => l.CuentaContable)
            .Where(a => a.EjercicioId == ejercicioId && a.EsTipoCierre)
            .OrderBy(a => a.Numero)
            .ToListAsync();

        return asientos.Select(a => new AsientoContableDto
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
            Lineas = a.LineasAsiento.Select(l => new LineaAsientoDto
            {
                Id = l.Id,
                CuentaContableId = l.CuentaContableId,
                CodigoCuenta = l.CuentaContable.Codigo,
                NombreCuenta = l.CuentaContable.Nombre,
                CentroCostoId = l.CentroCostoId,
                Debe = l.Debe,
                Haber = l.Haber,
                Moneda = l.Moneda,
                TipoCambio = l.TipoCambio,
                ImporteMonedaBase = l.ImporteMonedaBase
            }).ToList(),
            TotalDebe = a.LineasAsiento.Sum(l => l.Debe),
            TotalHaber = a.LineasAsiento.Sum(l => l.Haber)
        }).ToList();
    }

    private async Task<CuentaContable> ObtenerOCrearCuentaCierre(
        Guid planId, string codigo, string nombre, string tipo, string naturaleza)
    {
        var cuenta = await _context.CuentasContables
            .FirstOrDefaultAsync(c => c.PlanCuentasId == planId && c.Nombre == nombre);

        if (cuenta is not null)
            return cuenta;

        // Verificar que el código no esté ocupado; si lo está, generar uno único
        var codigoFinal = codigo;
        if (await _context.CuentasContables.AnyAsync(c => c.PlanCuentasId == planId && c.Codigo == codigoFinal))
            codigoFinal = codigo + "-" + Guid.NewGuid().ToString("N")[..4];

        cuenta = new CuentaContable
        {
            Id = Guid.NewGuid(),
            PlanCuentasId = planId,
            CuentaPadreId = null,
            Codigo = codigoFinal,
            Nombre = nombre,
            Tipo = tipo,
            Naturaleza = naturaleza,
            EsImputable = true,
            EsSistema = true,
            Estado = "Activa"
        };

        _context.CuentasContables.Add(cuenta);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Cuenta de cierre creada automáticamente | Nombre: {Nombre} | Código: {Codigo} | PlanId: {PlanId}",
            nombre, codigoFinal, planId);

        return cuenta;
    }
}
