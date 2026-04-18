namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando no se encuentra un recurso solicitado en la base de datos.
/// HTTP 404 - Not Found.
/// </summary>
public class EntidadNoEncontradaException : Exception
{
    private const string MensajePorDefecto =
     "El recurso solicitado no fue encontrado.";

    public EntidadNoEncontradaException()
  : base(MensajePorDefecto)
    {
    }

    public EntidadNoEncontradaException(string mensaje)
: base(mensaje)
    {
    }

    public EntidadNoEncontradaException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }

    public EntidadNoEncontradaException(string entidad, Guid id)
        : base($"No se encontró {entidad} con ID '{id}'.")
    {
    }
}
