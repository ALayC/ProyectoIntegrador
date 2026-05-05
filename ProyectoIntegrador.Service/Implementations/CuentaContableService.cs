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
        await ValidarPlanExiste(planCuentasId);

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
            Estado = dto.Estado
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
        await ValidarPlanExiste(planCuentasId);

        var cuentas = await _cuentaRepository.ObtenerPorPlanPaginado(planCuentasId, pagina, cantidadPorPagina);
        var total = await _cuentaRepository.ContarPorPlanDeCuentas(planCuentasId);

        var cuentasDto = cuentas.Select(Mapear).ToList();
        return new PaginadoDto<CuentaContableDto>(cuentasDto, pagina, cantidadPorPagina, total);
    }

    public async Task<List<CuentaContableArbolDto>> ObtenerArbolDeCuentas(Guid planId)
    {
        await ValidarPlanExiste(planId);

        var cuentas = await _cuentaRepository.ObtenerTodasPorPlan(planId);

        var dict = cuentas.ToDictionary(
            c => c.Id,
            c => new CuentaContableArbolDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Hijas = new List<CuentaContableArbolDto>()
            });

        var raiz = new List<CuentaContableArbolDto>();

        foreach (var cuenta in cuentas)
        {
            if (cuenta.CuentaPadreId == null)
            {
                raiz.Add(dict[cuenta.Id]);
            }
            else if (dict.ContainsKey(cuenta.CuentaPadreId.Value))
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

        if (await _cuentaRepository.ExisteCodigo(cuenta.PlanCuentasId, dto.Codigo))
        {
            var existente = await _cuentaRepository.ObtenerPorCodigo(cuenta.PlanCuentasId, dto.Codigo);
            if (existente is not null && existente.Id != id)
            {
                throw new CuentaDuplicadaException(cuenta.PlanCuentasId, dto.Codigo);
            }
        }

        Guid? cuentaPadreId = null;
        if (dto.CuentaPadreId.HasValue)
        {
            if (dto.CuentaPadreId.Value == id)
            {
                throw new InvalidOperationException("La cuenta no puede ser su propia cuenta padre.");
            }

            if (await EsDescendiente(id, dto.CuentaPadreId.Value))
            {
                throw new InvalidOperationException("La cuenta padre no puede ser un descendiente de la cuenta actual.");
            }

            var cuentaPadre = await _cuentaRepository.ObtenerPorId(dto.CuentaPadreId.Value)
                ?? throw new EntidadNoEncontradaException("CuentaPadre", dto.CuentaPadreId.Value);

            if (cuentaPadre.PlanCuentasId != cuenta.PlanCuentasId)
            {
                throw new AccesoNoAutorizadoException("La cuenta padre no pertenece al mismo plan de cuentas.");
            }

            cuentaPadreId = cuentaPadre.Id;
        }

        cuenta.Codigo = dto.Codigo;
        cuenta.Nombre = dto.Nombre;
        cuenta.Tipo = dto.Tipo;
        cuenta.Naturaleza = dto.Naturaleza;
        cuenta.EsImputable = dto.EsImputable;
        cuenta.Estado = dto.Estado;
        cuenta.CuentaPadreId = cuentaPadreId;

        await _cuentaRepository.Actualizar(cuenta);
        return Mapear(cuenta);
    }

    public async Task Desactivar(Guid id)
    {
        var cuenta = await _cuentaRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("CuentaContable", id);

        if (await _cuentaRepository.TieneMovimientos(id))
        {
            throw new CuentaConMovimientosException(id);
        }

        cuenta.Estado = "Inactiva";
        await _cuentaRepository.Actualizar(cuenta);
    }

    private static CuentaContableDto Mapear(CuentaContable cuenta) => new()
    {
        Id = cuenta.Id,
        Codigo = cuenta.Codigo,
        Nombre = cuenta.Nombre,
        Tipo = cuenta.Tipo,
        Naturaleza = cuenta.Naturaleza,
        EsImputable = cuenta.EsImputable,
        Estado = cuenta.Estado,
        CuentaPadreId = cuenta.CuentaPadreId
    };

    private async Task<bool> EsDescendiente(Guid cuentaId, Guid posibleDescendienteId, Guid planId)
    {
        var cuentas = await _cuentaRepository.ObtenerTodasPorPlan(planId);

        var hijosPorPadre = cuentas
            .Where(c => c.CuentaPadreId.HasValue)
            .GroupBy(c => c.CuentaPadreId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

        var pendientes = new Queue<Guid>();
        pendientes.Enqueue(cuentaId);

        while (pendientes.Count > 0)
        {
            var actualId = pendientes.Dequeue();

            if (!hijosPorPadre.ContainsKey(actualId))
                continue;

            foreach (var hijaId in hijosPorPadre[actualId])
            {
                if (hijaId == posibleDescendienteId)
                    return true;

                pendientes.Enqueue(hijaId);
            }
        }

        return false;
    }

    private async Task ValidarPlanExiste(Guid planCuentasId)
    {
        var plan = await _planDeCuentasRepository.ObtenerPorId(planCuentasId) ?? throw new EntidadNoEncontradaException("PlanDeCuentas", planCuentasId);
    }
}
