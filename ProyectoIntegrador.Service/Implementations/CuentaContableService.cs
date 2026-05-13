using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class CuentaContableService : ICuentaContableService
{
    private readonly ICuentaContableRepository _cuentaRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;

    public CuentaContableService(ICuentaContableRepository cuentaRepository, IPlanDeCuentasRepository planDeCuentasRepository)
    {
        _cuentaRepository = cuentaRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
    }

    public async Task<CuentaContableDto> Crear(Guid planCuentasId, CrearCuentaContableDto dto)
    {
        await ObtenerPlanExistente(planCuentasId);

        if (await _cuentaRepository.ExisteCodigo(planCuentasId, dto.Codigo))
        {
            throw new CuentaDuplicadaException(planCuentasId, dto.Codigo);
        }

        Guid? cuentaPadreId = null;
        if (dto.CuentaPadreId.HasValue)
        {
            var cuentaPadre = await _cuentaRepository.ObtenerPorId(dto.CuentaPadreId.Value)
                ?? throw new EntidadNoEncontradaException("CuentaPadre", dto.CuentaPadreId.Value);

            if (cuentaPadre.PlanCuentasId != planCuentasId)
            {
                throw new AccesoNoAutorizadoException("La cuenta padre no pertenece al mismo plan de cuentas.");
            }

            if (cuentaPadre.Estado == "Inactiva")
            {
                throw new ValidacionException("No se pueden crear subcuentas bajo una cuenta inactiva.");
            }

            if (cuentaPadre.EsImputable)
            {
                throw new CuentaJerarquiaInvalidaException("La cuenta padre debe ser no imputable.");
            }

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
        return Mapear(cuenta);
    }

    public async Task<CuentaContableDto> ObtenerPorId(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        return Mapear(cuenta);
    }

    public async Task<PaginadoDto<CuentaContableDto>> ObtenerPorPlanPaginado(Guid planCuentasId, int pagina, int cantidadPorPagina)
    {
        await ObtenerPlanExistente(planCuentasId);

        var cuentas = await _cuentaRepository.ObtenerPorPlanPaginado(planCuentasId, pagina, cantidadPorPagina);
        var total = await _cuentaRepository.ContarPorPlanDeCuentas(planCuentasId);

        var cuentasDto = cuentas.Select(Mapear).ToList();
        return new PaginadoDto<CuentaContableDto>(cuentasDto, pagina, cantidadPorPagina, total);
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
            {
                raiz.Add(dict[cuenta.Id]);
            }
            else
            {
                dict[cuenta.CuentaPadreId.Value].Hijas.Add(dict[cuenta.Id]);
            }
        }

        return raiz;
    }

    public async Task<CuentaContableDto> Actualizar(Guid id, ActualizarCuentaContableDto dto)
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
        {
            throw new CuentaDuplicadaException(cuenta.PlanCuentasId, dto.Codigo);
        }

        if (!cuenta.EsImputable && dto.EsImputable)
        {
            var hijas = await _cuentaRepository.ObtenerHijas(id);
            if (hijas.Count > 0)
            {
                throw new CuentaJerarquiaInvalidaException("No se puede marcar como imputable una cuenta con subcuentas.");
            }
        }

        cuenta.Codigo = dto.Codigo;
        cuenta.Nombre = dto.Nombre;
        cuenta.Tipo = dto.Tipo;
        cuenta.Naturaleza = dto.Naturaleza;
        cuenta.EsImputable = dto.EsImputable;
        // Estado no se modifica desde Actualizar; se gestiona en Activar/Desactivar.

        await _cuentaRepository.Actualizar(cuenta);
        return Mapear(cuenta);
    }

    public async Task Desactivar(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (cuenta.EsSistema)
        {
            throw new ValidacionException("No se pueden desactivar cuentas del sistema.");
        }

        var hijas = await _cuentaRepository.ObtenerHijas(id);
        if (hijas.Any(hija => hija.Estado == "Activa"))
        {
            throw new CuentaJerarquiaInvalidaException("No se puede desactivar una cuenta con subcuentas activas.");
        }

        if (await _cuentaRepository.TieneMovimientos(id))
        {
            throw new CuentaConMovimientosException(id);
        }

        cuenta.Estado = "Inactiva";
        await _cuentaRepository.Actualizar(cuenta);
    }

    public async Task Activar(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (cuenta.EsSistema)
        {
            throw new ValidacionException("No se pueden activar cuentas del sistema.");
        }

        if (cuenta.CuentaPadreId.HasValue)
        {
            var cuentaPadre = await _cuentaRepository.ObtenerPorId(cuenta.CuentaPadreId.Value)
                ?? throw new EntidadNoEncontradaException("CuentaPadre", cuenta.CuentaPadreId.Value);

            if (cuentaPadre.Estado == "Inactiva")
            {
                throw new ValidacionException("No se puede activar una cuenta cuyo padre está inactivo.");
            }
        }

        cuenta.Estado = "Activa";
        await _cuentaRepository.Actualizar(cuenta);
    }

    private static CuentaContableDto Mapear(CuentaContable cuenta) => new()
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

    private async Task ObtenerPlanExistente(Guid planCuentasId)
    {
        if (await _planDeCuentasRepository.ObtenerPorId(planCuentasId) is null)
        {
            throw new EntidadNoEncontradaException("PlanDeCuentas", planCuentasId);
        }
    }


}
