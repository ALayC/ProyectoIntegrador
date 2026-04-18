namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando un ejercicio contable se solapa con otro ejercicio existente del mismo cliente.
/// HTTP 400 - Bad Request.
/// </summary>
public class EjercicioSolapadoException : Exception
{
    private const string MensajePorDefecto =
        "El ejercicio contable se solapa con otro ejercicio existente del mismo cliente.";

    public EjercicioSolapadoException()
 : base(MensajePorDefecto)
    {
    }

    public EjercicioSolapadoException(string mensaje)
        : base(mensaje)
  {
    }

    public EjercicioSolapadoException(string mensaje, Exception innerException)
: base(mensaje, innerException)
    {
    }

    public EjercicioSolapadoException(DateTime fechaInicio, DateTime fechaFin)
        : base($"El ejercicio contable ({fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}) se solapa con otro ejercicio existente del mismo cliente.")
    {
    }
}
