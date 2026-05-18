namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando no se encuentra un recurso por su identificador.
/// HTTP 404 - Not Found.
/// </summary>
public class EntidadNoEncontradaException : Exception
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }

    public EntidadNoEncontradaException(string entidad, Guid id)
        : base($"No se encontró '{entidad}' con ID: {id}.") { }
}
