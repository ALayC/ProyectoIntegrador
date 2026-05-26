namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta desactivar una cuenta con movimientos asociados.
/// HTTP 400 - Bad Request.
/// </summary>
public class CuentaConMovimientosException : Exception
{
    private const string MensajePorDefecto =
        "No se puede desactivar una cuenta con movimientos registrados.";

    public CuentaConMovimientosException()
        : base(MensajePorDefecto)
    {
    }

    public CuentaConMovimientosException(string mensaje)
        : base(mensaje)
    {
    }

    public CuentaConMovimientosException(Guid cuentaId)
        : base(MensajePorDefecto)
    {
    }
}
