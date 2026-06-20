using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Constants;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class CuentaContableService : ICuentaContableService
{
    private readonly ICuentaContableRepository _cuentaRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<CuentaContableService> _logger;

    public CuentaContableService(
        ICuentaContableRepository cuentaRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        IAuditoriaService auditoriaService,
        ILogger<CuentaContableService> logger)
    {
        _cuentaRepository = cuentaRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    public async Task<CuentaContableResponseDto> Crear(Guid planCuentasId, CrearCuentaContableDto dto, Guid usuarioId)
    {
        await ObtenerPlanExistente(planCuentasId);

        if (await _cuentaRepository.ExisteCodigo(planCuentasId, dto.Codigo))
        {
            _logger.LogWarning("Intento de crear cuenta con codigo duplicado: {Codigo} | PlanId: {PlanId} | UsuarioId: {UsuarioId}", dto.Codigo, planCuentasId, usuarioId);
            throw new CuentaDuplicadaException(planCuentasId, dto.Codigo);
        }

        Guid? cuentaPadreId = null;
        if (dto.CuentaPadreId.HasValue)
        {
            var cuentaPadre = await _cuentaRepository.ObtenerPorId(dto.CuentaPadreId.Value)
                ?? throw new EntidadNoEncontradaException("CuentaPadre", dto.CuentaPadreId.Value);

            if (cuentaPadre.PlanCuentasId != planCuentasId)
                throw new AccesoNoAutorizadoException("La cuenta padre no pertenece al mismo plan de cuentas.");

            if (cuentaPadre.Estado == "Inactiva")
                throw new ValidacionException("No se pueden crear subcuentas bajo una cuenta inactiva.");

            if (cuentaPadre.EsImputable)
                throw new CuentaJerarquiaInvalidaException("La cuenta padre debe ser no imputable.");

            cuentaPadreId = cuentaPadre.Id;
        }

        var cuenta = new CuentaContable
        {
            Id = Guid.NewGuid(),
            PlanCuentasId = planCuentasId,
            CuentaPadreId = cuentaPadreId,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Naturaleza = dto.Naturaleza,
            EsImputable = dto.EsImputable,
            EsSistema = false,
            Estado = "Activa"
        };

        await _cuentaRepository.Guardar(cuenta);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.CuentaContable,
            AuditoriaConstantes.Acciones.Crear,
            datosAnteriores: null,
            datosNuevos: ConstruirDatosAuditoria(cuenta));

        _logger.LogInformation("Cuenta contable creada | Codigo: {Codigo} | PlanId: {PlanId} | UsuarioId: {UsuarioId}", cuenta.Codigo, planCuentasId, usuarioId);
        return Mapear(cuenta);
    }

    public async Task<CuentaContableResponseDto> ObtenerPorId(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        return Mapear(cuenta);
    }

    public async Task<PaginadoDto<CuentaContableResponseDto>> ObtenerPorPlanPaginado(Guid planCuentasId, int pagina, int cantidadPorPagina)
    {
        await ObtenerPlanExistente(planCuentasId);

        var cuentas = await _cuentaRepository.ObtenerPorPlanPaginado(planCuentasId, pagina, cantidadPorPagina);
        var total = await _cuentaRepository.ContarPorPlanDeCuentas(planCuentasId);

        var cuentasDto = cuentas.Select(Mapear).ToList();
        return new PaginadoDto<CuentaContableResponseDto>(cuentasDto, pagina, cantidadPorPagina, total);
    }

    public async Task<List<CuentaContableArbolDto>> ObtenerArbolDeCuentas(Guid planId)
    {
        await ObtenerPlanExistente(planId);

        var cuentas = await _cuentaRepository.ObtenerTodasPorPlan(planId);

        var dict = cuentas.ToDictionary(
            c => c.Id,
            c => new CuentaContableArbolDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                EsSistema = c.EsSistema,
                EsImputable = c.EsImputable,
                Estado = c.Estado,
                Hijas = new List<CuentaContableArbolDto>()
            });

        var raiz = new List<CuentaContableArbolDto>();

        foreach (var cuenta in cuentas)
        {
            if (cuenta.CuentaPadreId == null || !dict.ContainsKey(cuenta.CuentaPadreId.Value))
                raiz.Add(dict[cuenta.Id]);
            else
                dict[cuenta.CuentaPadreId.Value].Hijas.Add(dict[cuenta.Id]);
        }

        return raiz;
    }

    public async Task<List<CuentaContableResponseDto>> ObtenerImputables(Guid planCuentasId)
    {
        await ObtenerPlanExistente(planCuentasId);

        var cuentas = await _cuentaRepository.ObtenerImputables(planCuentasId);
        return cuentas.Select(Mapear).ToList();
    }

    public async Task<CuentaContableResponseDto> Actualizar(Guid id, ActualizarCuentaContableDto dto, Guid usuarioId)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (cuenta.EsSistema && (dto.Codigo != cuenta.Codigo
            || dto.Tipo != cuenta.Tipo
            || dto.Naturaleza != cuenta.Naturaleza
            || dto.EsImputable != cuenta.EsImputable))
        {
            throw new ValidacionException("No se permite modificar propiedades estructurales de cuentas del sistema.");
        }

        var existente = await _cuentaRepository.ObtenerPorCodigo(cuenta.PlanCuentasId, dto.Codigo);

        if (existente is not null && existente.Id != id)
            throw new CuentaDuplicadaException(cuenta.PlanCuentasId, dto.Codigo);

        if (!cuenta.EsImputable && dto.EsImputable)
        {
            var hijas = await _cuentaRepository.ObtenerHijas(id);
            if (hijas.Count > 0)
                throw new CuentaJerarquiaInvalidaException("No se puede marcar como imputable una cuenta con subcuentas.");
        }

        var datosAnteriores = ConstruirDatosAuditoria(cuenta);

        cuenta.Codigo = dto.Codigo;
        cuenta.Nombre = dto.Nombre;
        cuenta.Tipo = dto.Tipo;
        cuenta.Naturaleza = dto.Naturaleza;
        cuenta.EsImputable = dto.EsImputable;

        await _cuentaRepository.Actualizar(cuenta);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.CuentaContable,
            AuditoriaConstantes.Acciones.Editar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(cuenta));

        _logger.LogInformation("Cuenta contable actualizada | Id: {CuentaId} | Codigo: {Codigo} | UsuarioId: {UsuarioId}", id, cuenta.Codigo, usuarioId);
        return Mapear(cuenta);
    }

    public async Task Desactivar(Guid id, Guid usuarioId)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (cuenta.EsSistema)
            throw new ValidacionException("No se pueden desactivar cuentas del sistema.");

        var hijas = await _cuentaRepository.ObtenerHijas(id);
        if (hijas.Any(hija => hija.Estado == "Activa"))
            throw new CuentaJerarquiaInvalidaException("No se puede desactivar una cuenta con subcuentas activas.");

        if (await _cuentaRepository.TieneMovimientos(id))
            throw new CuentaConMovimientosException(id);

        var datosAnteriores = ConstruirDatosAuditoria(cuenta);

        cuenta.Estado = "Inactiva";
        await _cuentaRepository.Actualizar(cuenta);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.CuentaContable,
            AuditoriaConstantes.Acciones.Desactivar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(cuenta));

        _logger.LogInformation("Cuenta contable desactivada | Id: {CuentaId} | Codigo: {Codigo} | UsuarioId: {UsuarioId}", id, cuenta.Codigo, usuarioId);
    }

    public async Task Activar(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (cuenta.EsSistema)
            throw new ValidacionException("No se pueden activar cuentas del sistema.");

        if (cuenta.CuentaPadreId.HasValue)
        {
            var cuentaPadre = await _cuentaRepository.ObtenerPorId(cuenta.CuentaPadreId.Value)
                ?? throw new EntidadNoEncontradaException("CuentaPadre", cuenta.CuentaPadreId.Value);

            if (cuentaPadre.Estado == "Inactiva")
                throw new ValidacionException("No se puede activar una cuenta cuyo padre está inactivo.");
        }

        cuenta.Estado = "Activa";
        await _cuentaRepository.Actualizar(cuenta);
        _logger.LogInformation("Cuenta contable activada | Id: {CuentaId} | Codigo: {Codigo}", id, cuenta.Codigo);
    }

    public async Task<string> SiguienteCodigoHija(Guid cuentaPadreId)
    {
        var padre = await _cuentaRepository.ObtenerPorId(cuentaPadreId)
            ?? throw new EntidadNoEncontradaException("CuentaContable", cuentaPadreId);

        var hijas = await _cuentaRepository.ObtenerHijas(cuentaPadreId);

        var prefijo = padre.Codigo;
        var maxSufijo = hijas
            .Select(h =>
            {
                if (h.Codigo.StartsWith(prefijo + ".") &&
                    int.TryParse(h.Codigo[(prefijo.Length + 1)..].Split('.')[0], out var n))
                    return n;
                return 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefijo}.{maxSufijo + 1}";
    }

    public async Task<string> SiguienteCodigoHija(Guid cuentaPadreId)
    {
        var padre = await _cuentaRepository.ObtenerPorId(cuentaPadreId)
            ?? throw new EntidadNoEncontradaException("CuentaContable", cuentaPadreId);

        var hijas = await _cuentaRepository.ObtenerHijas(cuentaPadreId);

        var prefijo = padre.Codigo;
        var maxSufijo = hijas
            .Select(h =>
            {
                if (h.Codigo.StartsWith(prefijo + ".") &&
                    int.TryParse(h.Codigo[(prefijo.Length + 1)..].Split('.')[0], out var n))
                    return n;
                return 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefijo}.{maxSufijo + 1}";
    }

    // ──────────────────────────────────────────────
    // Métodos privados
    // ──────────────────────────────────────────────

    private static CuentaContableResponseDto Mapear(CuentaContable cuenta) => new()
    {
        Id = cuenta.Id,
        PlanCuentasId = cuenta.PlanCuentasId,
        Codigo = cuenta.Codigo,
        Nombre = cuenta.Nombre,
        Tipo = cuenta.Tipo,
        Naturaleza = cuenta.Naturaleza,
        EsImputable = cuenta.EsImputable,
        EsSistema = cuenta.EsSistema,
        Estado = cuenta.Estado,
        CuentaPadreId = cuenta.CuentaPadreId
    };

    private static object ConstruirDatosAuditoria(CuentaContable cuenta) => new
    {
        cuenta.Id,
        cuenta.PlanCuentasId,
        cuenta.CuentaPadreId,
        cuenta.Codigo,
        cuenta.Nombre,
        cuenta.Tipo,
        cuenta.Naturaleza,
        cuenta.EsImputable,
        cuenta.EsSistema,
        cuenta.Estado
    };

    private async Task ObtenerPlanExistente(Guid planCuentasId)
    {
        if (await _planDeCuentasRepository.ObtenerPorId(planCuentasId) is null)
            throw new EntidadNoEncontradaException("PlanDeCuentas", planCuentasId);
    }
}
