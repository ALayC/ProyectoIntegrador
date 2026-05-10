using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class EjercicioContableService : IEjercicioContableService
{
    private readonly IEjercicioContableRepository _ejercicioRepository;
    private readonly IClienteRepository _clienteRepository;

    public EjercicioContableService(
        IEjercicioContableRepository ejercicioRepository,
        IClienteRepository clienteRepository)
    {
        _ejercicioRepository = ejercicioRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<EjercicioContableDto> Crear(CrearEjercicioContableDto dto)
    {
        await ValidarClienteExistente(dto.ClienteId);
        ValidarRangoFechas(dto.FechaInicio, dto.FechaFin);
         // Verificar si el cliente en la vida real lleva 2 ejercicios contables abiertos a la vez
        if (await _ejercicioRepository.ExisteSolapamiento(dto.ClienteId, dto.FechaInicio, dto.FechaFin))
        {
            throw new EjercicioSolapadoException(dto.FechaInicio.ToDateTime(TimeOnly.MinValue), dto.FechaFin.ToDateTime(TimeOnly.MinValue));
        }

        var abierto = await _ejercicioRepository.ObtenerAbiertoPorCliente(dto.ClienteId);
        if (abierto is not null)
        {
            throw new ValidacionException("Ya existe un ejercicio abierto para el cliente.");
        }

        var ejercicio = new EjercicioContable
        {
            Id = Guid.NewGuid(),
            ClienteId = dto.ClienteId,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = "Abierto"
        };

        await _ejercicioRepository.Guardar(ejercicio);
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

        ejercicio.Estado = "Cerrado";
        await _ejercicioRepository.Actualizar(ejercicio);
    }

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

    private async Task ValidarClienteExistente(Guid clienteId)
    {
        if (await _clienteRepository.ObtenerPorId(clienteId) is null)
        {
            throw new EntidadNoEncontradaException("Cliente", clienteId);
        }
    }
}
