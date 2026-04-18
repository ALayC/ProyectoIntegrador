namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta operar sobre un ejercicio contable que ya está cerrado.
/// HTTP 400 - Bad Request.
/// </summary>
public class EjercicioCerradoException : Exception
{
    private const string MensajePorDefecto =
       "No se puede operar sobre un ejercicio contable que ya está cerrado.";

    public EjercicioCerradoException()
        : base(MensajePorDefecto)
    {
    }

    public EjercicioCerradoException(string mensaje)
     : base(mensaje)
    {
    }

    public EjercicioCerradoException(string mensaje, Exception innerException)
    : base(mensaje, innerException)
    {
    }

    public EjercicioCerradoException(Guid ejercicioId)
        : base($"No se puede operar sobre el ejercicio contable '{ejercicioId}' porque ya está cerrado.")
    {
    }
}
