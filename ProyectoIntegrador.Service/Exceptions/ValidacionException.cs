namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza ante cualquier violación de regla de negocio genérica.
/// HTTP 400 - Bad Request.
/// </summary>
public class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}
