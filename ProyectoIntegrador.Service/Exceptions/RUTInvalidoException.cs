namespace ProyectoIntegrador.Service.Exceptions;

public class RUTInvalidoException : ValidacionException
{
    public RUTInvalidoException(string rut)
        : base($"El RUT '{rut}' no tiene un formato válido.")
    {
    }
}
