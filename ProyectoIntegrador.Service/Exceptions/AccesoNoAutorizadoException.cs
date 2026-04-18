namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando el usuario no tiene permisos para realizar la operación solicitada.
/// HTTP 403 - Forbidden.
/// </summary>
public class AccesoNoAutorizadoException : Exception
{
    private const string MensajePorDefecto =
        "No tiene permisos para realizar esta operación.";

    public AccesoNoAutorizadoException()
        : base(MensajePorDefecto)
    {
    }

    public AccesoNoAutorizadoException(string mensaje)
  : base(mensaje)
    {
    }

    public AccesoNoAutorizadoException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }

    public AccesoNoAutorizadoException(Guid usuarioId, string recurso)
 : base($"El usuario '{usuarioId}' no tiene permisos para acceder a '{recurso}'.")
    {
  }
}
