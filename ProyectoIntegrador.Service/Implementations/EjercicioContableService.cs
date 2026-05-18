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
    private readonly IAuditoriaService _auditoriaService;

    public EjercicioContableService(
        IEjercicioContableRepository ejercicioRepository,
        IClienteRepository clienteRepository,
        IAuditoriaService auditoriaService)
    {
        _ejercicioRepository = ejercicioRepository;
        _clienteRepository = clienteRepository;
        _auditoriaService = auditoriaService;
    }

    public async Task<EjercicioContableDto> Crear(CrearEjercicioContableDto dto)
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

        if (await _ejercicioRepository.ExisteSolapamiento(clienteId, fechaInicio, fechaFin))
        {
            throw new EjercicioSolapadoException(fechaInicio.ToDateTime(TimeOnly.MinValue), fechaFin.ToDateTime(TimeOnly.MinValue));
        }

        var abierto = await _ejercicioRepository.ObtenerAbiertoPorCliente(clienteId);
        if (abierto is not null)
        {
            throw new ValidacionException("Ya existe un ejercicio abierto para el cliente.");
        }

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

        return Mapear(ejercicio);
    }

    public async Task<EjercicioContableDto> ObtenerPorId(Guid id)
    {
        var ejercicio = await _ejercicioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", id);

        return Mapear(ejercicio);
    }

    public async Task<PaginadoDto<EjercicioContableDto>> ObtenerPorCliente(Guid clienteId, int pagina, int cantidadPorPagina)
    {
        if (pagina < 1 || cantidadPorPagina <= 0)
        {
            throw new ValidacionException("Los parámetros de paginación no son válidos.");
        }

        await ValidarClienteExistente(clienteId);

        var ejercicios = await _ejercicioRepository.ObtenerPorCliente(clienteId, pagina, cantidadPorPagina);
        var total = await _ejercicioRepository.ContarPorCliente(clienteId);

        var ejerciciosDto = ejercicios.Select(Mapear).ToList();
        return new PaginadoDto<EjercicioContableDto>(ejerciciosDto, pagina, cantidadPorPagina, total);
    }

    public async Task<EjercicioContableDto> Actualizar(Guid id, ActualizarEjercicioContableDto dto)
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

        if (await _ejercicioRepository.ExisteSolapamiento(ejercicio.ClienteId, fechaInicio, fechaFin, id))
        {
            throw new EjercicioSolapadoException(fechaInicio.ToDateTime(TimeOnly.MinValue), fechaFin.ToDateTime(TimeOnly.MinValue));
        }

        var abierto = await _ejercicioRepository.ObtenerAbiertoPorCliente(ejercicio.ClienteId);
        if (abierto is not null && abierto.Id != id)
        {
            throw new ValidacionException("Ya existe un ejercicio abierto para el cliente.");
        }

        ejercicio.FechaInicio = fechaInicio;
        ejercicio.FechaFin = fechaFin;

        await _ejercicioRepository.Actualizar(ejercicio);
        return Mapear(ejercicio);
    }

    public async Task Cerrar(Guid id, Guid usuarioId)
    {
        var ejercicio = await _ejercicioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("EjercicioContable", id);

        if (ejercicio.Estado == "Cerrado")
        {
            throw new EjercicioCerradoException(id);
        }

        var datosAnteriores = ConstruirDatosAuditoria(ejercicio);

        ejercicio.Estado = "Cerrado";
        await _ejercicioRepository.Actualizar(ejercicio);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.EjercicioContable,
            AuditoriaConstantes.Acciones.Cerrar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(ejercicio));
    }

    // ??????????????????????????????????????????????
    // Métodos privados
    // ??????????????????????????????????????????????

    private static EjercicioContableDto Mapear(EjercicioContable ejercicio) => new()
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
}
