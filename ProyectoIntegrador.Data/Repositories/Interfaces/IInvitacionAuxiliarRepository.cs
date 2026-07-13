using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IInvitacionAuxiliarRepository
{
    Task<InvitacionAuxiliar?> ObtenerPendientePorEmail(string email);
    Task<List<InvitacionAuxiliar>> ObtenerPorContador(Guid contadorId);
    Task Guardar(InvitacionAuxiliar invitacion);
    Task Actualizar(InvitacionAuxiliar invitacion);
}
