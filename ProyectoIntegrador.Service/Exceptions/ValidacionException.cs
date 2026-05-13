namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando hay un error de validación de entrada.
/// HTTP 400 - Bad Request.
/// </summary>
public class ValidacionException : Exception
{
    private const string MensajePorDefecto = "Los datos enviados no son válidos.";

    public ValidacionException()
        : base(MensajePorDefecto)
    {
    }

    public ValidacionException(string mensaje)
        : base(mensaje)
    {
    }
}
