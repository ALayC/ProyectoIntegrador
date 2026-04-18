namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando un asiento contable no está balanceado (suma de debe ? suma de haber).
/// HTTP 400 - Bad Request.
/// </summary>
public class AsientoNoBalanceadoException : Exception
{
    private const string MensajePorDefecto =
        "El asiento contable no está balanceado: la suma del debe no coincide con la suma del haber.";

    public AsientoNoBalanceadoException()
        : base(MensajePorDefecto)
    {
    }

    public AsientoNoBalanceadoException(string mensaje)
        : base(mensaje)
    {
    }

    public AsientoNoBalanceadoException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }

    public AsientoNoBalanceadoException(decimal totalDebe, decimal totalHaber)
        : base($"El asiento contable no está balanceado: Debe ({totalDebe:N2}) ? Haber ({totalHaber:N2}).")
    {
    }
}
