namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando un archivo Excel de importación tiene una estructura incorrecta o datos inválidos.
/// HTTP 400 - Bad Request.
/// </summary>
public class ImportacionInvalidaException : Exception
{
  private const string MensajePorDefecto =
        "El archivo de importación tiene una estructura incorrecta o contiene datos inválidos.";

    public ImportacionInvalidaException()
        : base(MensajePorDefecto)
    {
    }

  public ImportacionInvalidaException(string mensaje)
        : base(mensaje)
    {
    }

  public ImportacionInvalidaException(string mensaje, Exception innerException)
      : base(mensaje, innerException)
    {
    }

    public ImportacionInvalidaException(string nombreArchivo, string detalle)
  : base($"El archivo '{nombreArchivo}' no pudo importarse: {detalle}.")
    {
    }
}
