using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface IDispositivoConfiableRepository
{
    Task<DispositivoConfiable?> ObtenerPorToken(string token);
    Task Guardar(DispositivoConfiable dispositivo);
    Task EliminarExpiradosPorUsuario(Guid usuarioId);
}
