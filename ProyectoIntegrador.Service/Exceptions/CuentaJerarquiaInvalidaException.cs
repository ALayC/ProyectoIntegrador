namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando la jerarquía de cuentas contables es inválida.
/// HTTP 400 - Bad Request.
/// </summary>
public class CuentaJerarquiaInvalidaException : Exception
{
    private const string MensajePorDefecto =
        "La jerarquía de la cuenta contable es inválida.";

    public CuentaJerarquiaInvalidaException()
        : base(MensajePorDefecto)
    {
    }

    public CuentaJerarquiaInvalidaException(string mensaje)
        : base(mensaje)
    {
    }
}
