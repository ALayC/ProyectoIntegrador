using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Repositories.Interfaces;

public interface ITokenRevocadoRepository
{
    Task<TokenRevocado?> ObtenerPorId(Guid id);
    Task<bool> EstaRevocado(string token);
    Task Guardar(TokenRevocado tokenRevocado);
    Task EliminarExpirados();
}
