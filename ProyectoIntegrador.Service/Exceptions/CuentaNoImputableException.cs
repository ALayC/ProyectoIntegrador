namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta registrar un movimiento contra una cuenta contable que no es imputable (es agrupadora).
/// HTTP 400 - Bad Request.
/// </summary>
public class CuentaNoImputableException : Exception
{
    private const string MensajePorDefecto =
"No se pueden registrar movimientos contra una cuenta agrupadora. Solo las cuentas imputables admiten movimientos.";

    public CuentaNoImputableException()
    : base(MensajePorDefecto)
    {
    }

    public CuentaNoImputableException(string mensaje)
        : base(mensaje)
    {
    }

    public CuentaNoImputableException(string mensaje, Exception innerException)
 : base(mensaje, innerException)
    {
    }

    public CuentaNoImputableException(Guid cuentaId, string codigoCuenta)
        : base($"La cuenta '{codigoCuenta}' (ID: {cuentaId}) es agrupadora y no admite movimientos.")
    {
    }
}
