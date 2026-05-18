namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando la suma del Debe no es igual a la suma del Haber en un asiento contable.
/// HTTP 400 - Bad Request.
/// </summary>
public class AsientoDesbalanceadoException : Exception
{
    public AsientoDesbalanceadoException(decimal totalDebe, decimal totalHaber)
        : base($"El asiento está desbalanceado: Debe={totalDebe:N2}, Haber={totalHaber:N2}. Deben ser iguales.")
    {
    }
}