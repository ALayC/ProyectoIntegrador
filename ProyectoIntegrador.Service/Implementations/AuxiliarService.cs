using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class AuxiliarService : IAuxiliarService
{
    private readonly IInvitacionAuxiliarRepository _invitacionRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly UIOptions _uiOptions;
    private readonly ILogger<AuxiliarService> _logger;

    public AuxiliarService(
        IInvitacionAuxiliarRepository invitacionRepository,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        IOptions<UIOptions> uiOptions,
        ILogger<AuxiliarService> logger)
    {
        _invitacionRepository = invitacionRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _uiOptions = uiOptions.Value;
        _logger = logger;
    }

    public async Task<InvitacionAuxiliarResponseDto> InvitarAuxiliar(Guid contadorId, InvitarAuxiliarDto dto)
    {
        // Validar que el contador exista y tenga el rol correcto
        var contador = await _usuarioRepository.ObtenerPorId(contadorId)
            ?? throw new EntidadNoEncontradaException("Usuario", contadorId);

        if (contador.Rol.Nombre != "Contador" && contador.Rol.Nombre != "Administrador")
            throw new AccesoNoAutorizadoException(contadorId, "Invitar auxiliares");

        // Evitar invitar a alguien que ya es auxiliar del mismo contador
        var usuarioExistente = await _usuarioRepository.ObtenerPorEmail(dto.Email);
        if (usuarioExistente is not null && usuarioExistente.ContadorId == contadorId)
            throw new ValidacionException($"El usuario '{dto.Email}' ya es auxiliar de este contador.");

        // Crear la invitación (si ya hay una pendiente vigente la sobreescribimos con una nueva)
        var invitacion = new InvitacionAuxiliar
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLowerInvariant(),
            ContadorId = contadorId,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            Estado = "Pendiente"
        };

        await _invitacionRepository.Guardar(invitacion);

        try
        {
            await _emailService.EnviarInvitacionAuxiliarAsync(dto.Email, contador.NombreCompleto, _uiOptions.BaseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de invitaci\u00f3n auxiliar a: {Email}", dto.Email);
        }

        _logger.LogInformation("Invitaci\u00f3n auxiliar enviada a {Email} por contador {ContadorId}", dto.Email, contadorId);

        return Mapear(invitacion);
    }

    public async Task<List<InvitacionAuxiliarResponseDto>> ObtenerInvitaciones(Guid contadorId)
    {
        var invitaciones = await _invitacionRepository.ObtenerPorContador(contadorId);
        return invitaciones.Select(Mapear).ToList();
    }

    public async Task RevocarAuxiliar(Guid contadorId, Guid auxiliarId)
    {
        var auxiliar = await _usuarioRepository.ObtenerPorId(auxiliarId)
            ?? throw new EntidadNoEncontradaException("Usuario", auxiliarId);

        if (auxiliar.ContadorId != contadorId)
            throw new AccesoNoAutorizadoException(contadorId, "Revocar auxiliar ajeno");

        auxiliar.Estado = "Inactivo";
        await _usuarioRepository.Actualizar(auxiliar);

        _logger.LogInformation("Auxiliar {AuxiliarId} revocado por contador {ContadorId}", auxiliarId, contadorId);
    }

    private static InvitacionAuxiliarResponseDto Mapear(InvitacionAuxiliar inv) => new()
    {
        Id = inv.Id,
        Email = inv.Email,
        Estado = inv.Estado,
        FechaCreacion = inv.FechaCreacion,
        FechaExpiracion = inv.FechaExpiracion
    };
}
