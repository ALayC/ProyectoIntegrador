namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta crear un recurso que ya existe (RUT o email duplicado).
/// HTTP 409 - Conflict.
/// </summary>
public class DuplicadoException : Exception
{
    private const string MensajePorDefecto =
"Ya existe un registro con los datos proporcionados.";

    public DuplicadoException()
        : base(MensajePorDefecto)
    {
    }

    public DuplicadoException(string mensaje)
        : base(mensaje)
    {
  }

    public DuplicadoException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }

    public DuplicadoException(string campo, string valor)
        : base($"Ya existe un registro con {campo} '{valor}'.")
    {
    }
}
