using Microsoft.Extensions.Logging;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Constants;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class ComprobanteService : IComprobanteService
{
    private readonly IComprobanteRepository _comprobanteRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IAsientoContableService _asientoContableService;
    private readonly ILogger<ComprobanteService> _logger;

    public ComprobanteService(
        IComprobanteRepository comprobanteRepository,
        IClienteRepository clienteRepository,
        IAuditoriaService auditoriaService,
        IAsientoContableService asientoContableService,
        ILogger<ComprobanteService> logger)
    {
        _comprobanteRepository = comprobanteRepository;
        _clienteRepository = clienteRepository;
        _auditoriaService = auditoriaService;
        _asientoContableService = asientoContableService;
        _logger = logger;
    }

    public async Task<ComprobanteDetalleDto> Crear(ComprobanteCrearDto dto, Guid usuarioId)
    {
        var cliente = await _clienteRepository.ObtenerPorId(dto.ClienteId)
            ?? throw new EntidadNoEncontradaException("Cliente", dto.ClienteId);

        var rutNormalizado = ValidarRut(dto.RUT);
        ValidarFecha(dto.Fecha);
        ValidarImportes(dto.ImporteNeto, dto.TasaIVA, dto.ImporteIVA, dto.ImporteTotal);

        var tipo = ParsearTipo(dto.Tipo);

        var existeDuplicado = await _comprobanteRepository.ExisteDuplicado(
            dto.Numero,
            rutNormalizado,
            dto.Fecha,
            dto.ClienteId);

        if (existeDuplicado)
            throw new ComprobanteDuplicadoException(dto.Numero, rutNormalizado, dto.Fecha);

        var comprobante = new Comprobante
        {
            Id = Guid.NewGuid(),
            ClienteId = dto.ClienteId,
            Tipo = tipo,
            Numero = dto.Numero.Trim(),
            RUT = rutNormalizado,
            Fecha = dto.Fecha,
            ImporteNeto = dto.ImporteNeto,
            TasaIVA = dto.TasaIVA,
            ImporteIVA = dto.ImporteIVA,
            ImporteTotal = dto.ImporteTotal,
            Estado = EstadoComprobante.Activo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            DeletedAt = null
        };

        await _comprobanteRepository.Guardar(comprobante);

        var resultado = MapearDetalle(comprobante);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Crear,
            datosAnteriores: null,
            datosNuevos: resultado);

        _logger.LogInformation("Comprobante creado | Id: {ComprobanteId} | Tipo: {Tipo} | Nº: {Numero} | ClienteId: {ClienteId} | UsuarioId: {UsuarioId}",
            comprobante.Id, comprobante.Tipo, comprobante.Numero, comprobante.ClienteId, usuarioId);

        return resultado;
    }

    public async Task<ComprobanteDetalleDto> Modificar(Guid id, ComprobanteModificarDto dto, Guid usuarioId)
    {
        var comprobante = await _comprobanteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Comprobante", id);

        if (comprobante.AsientoId.HasValue)
            throw new ComprobanteConAsientoException(id);

        var datosAnteriores = MapearDetalle(comprobante);

        var rutNormalizado = ValidarRut(dto.RUT);
        ValidarFecha(dto.Fecha);
        ValidarImportes(dto.ImporteNeto, dto.TasaIVA, dto.ImporteIVA, dto.ImporteTotal);

        var tipo = ParsearTipo(dto.Tipo);

        var claveCambio =
            !string.Equals(comprobante.Numero, dto.Numero.Trim(), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(comprobante.RUT, rutNormalizado, StringComparison.OrdinalIgnoreCase) ||
            comprobante.Fecha != dto.Fecha;

        if (claveCambio)
        {
            var existeDuplicado = await _comprobanteRepository.ExisteDuplicado(
                dto.Numero,
                rutNormalizado,
                dto.Fecha,
                comprobante.ClienteId);

            if (existeDuplicado)
                throw new ComprobanteDuplicadoException(dto.Numero, rutNormalizado, dto.Fecha);
        }

        comprobante.Tipo = tipo;
        comprobante.Numero = dto.Numero.Trim();
        comprobante.RUT = rutNormalizado;
        comprobante.Fecha = dto.Fecha;
        comprobante.ImporteNeto = dto.ImporteNeto;
        comprobante.TasaIVA = dto.TasaIVA;
        comprobante.ImporteIVA = dto.ImporteIVA;
        comprobante.ImporteTotal = dto.ImporteTotal;
        comprobante.UpdatedAt = DateTime.UtcNow;

        await _comprobanteRepository.Actualizar(comprobante);

        var resultado = MapearDetalle(comprobante);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Editar,
            datosAnteriores: datosAnteriores,
            datosNuevos: resultado);

        _logger.LogInformation("Comprobante modificado | Id: {ComprobanteId} | UsuarioId: {UsuarioId}", id, usuarioId);

        return resultado;
    }

    public async Task Anular(Guid id, Guid usuarioId)
    {
        var comprobante = await _comprobanteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Comprobante", id);

        if (comprobante.AsientoId.HasValue)
            throw new ComprobanteConAsientoException(id);

        var datosAnteriores = MapearDetalle(comprobante);

        await _comprobanteRepository.Anular(id);

        var comprobanteAnulado = await _comprobanteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Comprobante", id);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Anular,
            datosAnteriores: datosAnteriores,
            datosNuevos: MapearDetalle(comprobanteAnulado));

        _logger.LogInformation("Comprobante anulado | Id: {ComprobanteId} | UsuarioId: {UsuarioId}", id, usuarioId);
    }

    public async Task<ComprobanteDetalleDto> Obtener(Guid id)
    {
        var comprobante = await _comprobanteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Comprobante", id);

        return MapearDetalle(comprobante);
    }

    public async Task<List<ComprobanteResumenDto>> Listar(FiltroComprobanteDto filtro)
    {
        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue && filtro.FechaDesde > filtro.FechaHasta)
            throw new FechaFueraDeRangoException(filtro.FechaDesde.Value);

        if (filtro.Pagina <= 0 || filtro.CantidadPorPagina <= 0)
            throw new ValidacionException("La paginación es inválida.");

        var tipo = ParsearTipoOpcional(filtro.Tipo);
        var estado = ParsearEstadoOpcional(filtro.Estado);

        var rutNormalizado = string.IsNullOrWhiteSpace(filtro.RUT)
            ? null
            : ValidarRut(filtro.RUT);

        var comprobantes = await _comprobanteRepository.ObtenerPorFiltros(
            filtro.ClienteId,
            tipo,
            rutNormalizado,
            filtro.FechaDesde,
            filtro.FechaHasta,
            estado,
            filtro.Pagina,
            filtro.CantidadPorPagina);

        return comprobantes
            .Select(MapearResumen)
            .ToList();
    }

    public async Task<ComprobanteDetalleDto> ObtenerPorAsiento(Guid asientoId)
    {
        var comprobante = await _comprobanteRepository.ObtenerPorAsiento(asientoId)
            ?? throw new EntidadNoEncontradaException("Comprobante para asiento", asientoId);

        return MapearDetalle(comprobante);
    }

    public async Task<AsientoContableDto> GenerarAsiento(Guid id, GenerarAsientoDesdeComprobanteDto dto, Guid usuarioId)
    {
        var comprobante = await _comprobanteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Comprobante", id);

        if (comprobante.Estado == EstadoComprobante.Anulado)
            throw new ValidacionException("No se puede generar asiento desde un comprobante anulado.");

        if (comprobante.AsientoId.HasValue)
            throw new ValidacionException("El comprobante ya tiene un asiento asociado.");

        if (dto.CuentaDebeId == dto.CuentaHaberId)
            throw new ValidacionException("Las cuentas de debe y haber deben ser distintas.");

        var datosAnteriores = MapearDetalle(comprobante);

        var asientoCreado = await _asientoContableService.Crear(new CrearAsientoContableDto
        {
            ClienteId = comprobante.ClienteId,
            EjercicioId = dto.EjercicioId,
            Fecha = dto.Fecha ?? comprobante.Fecha,
            Glosa = string.IsNullOrWhiteSpace(dto.Glosa)
                ? $"Asiento generado desde comprobante {comprobante.Tipo} {comprobante.Numero}"
                : dto.Glosa,
            Lineas = new List<LineaAsientoInputDto>
            {
                new()
                {
                    CuentaContableId = dto.CuentaDebeId,
                    Debe = comprobante.ImporteTotal,
                    Haber = 0,
                    Moneda = "UYU",
                    TipoCambio = 1m
                },
                new()
                {
                    CuentaContableId = dto.CuentaHaberId,
                    Debe = 0,
                    Haber = comprobante.ImporteTotal,
                    Moneda = "UYU",
                    TipoCambio = 1m
                }
            }
        }, usuarioId);

        comprobante.AsientoId = asientoCreado.Id;
        comprobante.UpdatedAt = DateTime.UtcNow;
        await _comprobanteRepository.Actualizar(comprobante);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Comprobante,
            AuditoriaConstantes.Acciones.Confirmar,
            datosAnteriores: datosAnteriores,
            datosNuevos: MapearDetalle(comprobante));

        _logger.LogInformation("Asiento generado desde comprobante | ComprobanteId: {ComprobanteId} | AsientoId: {AsientoId} | UsuarioId: {UsuarioId}",
            id, asientoCreado.Id, usuarioId);

        return asientoCreado;
    }

    private static ComprobanteDetalleDto MapearDetalle(Comprobante c)
    {
        return new ComprobanteDetalleDto
        {
            Id = c.Id,
            ClienteId = c.ClienteId,
            Tipo = c.Tipo.ToString(),
            Numero = c.Numero,
            RUT = c.RUT,
            Fecha = c.Fecha,
            ImporteNeto = c.ImporteNeto,
            TasaIVA = c.TasaIVA,
            ImporteIVA = c.ImporteIVA,
            ImporteTotal = c.ImporteTotal,
            Estado = c.Estado.ToString(),
            AsientoId = c.AsientoId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            DeletedAt = c.DeletedAt
        };
    }

    private static ComprobanteResumenDto MapearResumen(Comprobante c)
    {
        return new ComprobanteResumenDto
        {
            Id = c.Id,
            ClienteId = c.ClienteId,
            Tipo = c.Tipo.ToString(),
            Numero = c.Numero,
            RUT = c.RUT,
            Fecha = c.Fecha,
            ImporteTotal = c.ImporteTotal,
            Estado = c.Estado.ToString(),
            AsientoId = c.AsientoId
        };
    }

    private static TipoComprobante ParsearTipo(string tipo)
    {
        if (Enum.TryParse<TipoComprobante>(tipo, true, out var valor))
            return valor;

        throw new ValidacionException("El tipo de comprobante no es válido.");
    }

    private static TipoComprobante? ParsearTipoOpcional(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return null;

        if (Enum.TryParse<TipoComprobante>(tipo, true, out var valor))
            return valor;

        throw new ValidacionException("El filtro de tipo de comprobante no es válido.");
    }

    private static EstadoComprobante? ParsearEstadoOpcional(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return null;

        if (Enum.TryParse<EstadoComprobante>(estado, true, out var valor))
            return valor;

        throw new ValidacionException("El filtro de estado de comprobante no es válido.");
    }

    private static void ValidarFecha(DateOnly fecha)
    {
        if (fecha == default)
            throw new FechaFueraDeRangoException(fecha);
    }

    private static string ValidarRut(string rut)
    {
        var normalizado = new string(rut.Where(char.IsDigit).ToArray());

        if (normalizado.Length != 12)
            throw new RUTInvalidoException(rut);

        return normalizado;
    }

    private static void ValidarImportes(decimal importeNeto, decimal tasaIva, decimal importeIva, decimal importeTotal)
    {
        if (importeNeto <= 0)
            throw new ValidacionException("El importe neto debe ser mayor a cero.");

        if (tasaIva < 0)
            throw new ValidacionException("La tasa de IVA no puede ser negativa.");

        if (importeIva < 0)
            throw new ValidacionException("El importe IVA no puede ser negativo.");

        if (importeTotal <= 0)
            throw new ValidacionException("El importe total debe ser mayor a cero.");

        if (importeNeto + importeIva != importeTotal)
            throw new ValidacionException("El importe total debe ser igual a neto + IVA.");
    }
}
