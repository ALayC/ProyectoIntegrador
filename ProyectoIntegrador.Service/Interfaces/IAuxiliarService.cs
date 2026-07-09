using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IAuxiliarService
{
    /// <summary>El contador invita a un email como su auxiliar. Devuelve la invitación creada.</summary>
    Task<InvitacionAuxiliarResponseDto> InvitarAuxiliar(Guid contadorId, InvitarAuxiliarDto dto);

    /// <summary>Lista todas las invitaciones y auxiliares activos del contador.</summary>
    Task<List<InvitacionAuxiliarResponseDto>> ObtenerInvitaciones(Guid contadorId);

    /// <summary>El contador revoca el acceso de un auxiliar (lo deja Inactivo).</summary>
    Task RevocarAuxiliar(Guid contadorId, Guid auxiliarId);
}
