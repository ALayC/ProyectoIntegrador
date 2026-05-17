namespace ProyectoIntegrador.Service.Interfaces;

public interface IAuditoriaService
{
    /// <summary>
    /// Registra un evento de auditoría serializando los datos anteriores y nuevos a JSON.
    /// </summary>
    Task Registrar(Guid usuarioId, string entidad, string accion, object? datosAnteriores, object? datosNuevos);
}
