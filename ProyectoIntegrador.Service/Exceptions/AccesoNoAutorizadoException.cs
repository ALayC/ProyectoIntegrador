namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando el usuario no tiene permisos para realizar la operación.
/// HTTP 403 - Forbidden.
/// </summary>
public class AccesoNoAutorizadoException : Exception
{
    public AccesoNoAutorizadoException(string mensaje) : base(mensaje) { }

    public AccesoNoAutorizadoException(Guid usuarioId, string accion)
        : base($"El usuario '{usuarioId}' no tiene permisos para realizar la acción '{accion}'.") { }
}
