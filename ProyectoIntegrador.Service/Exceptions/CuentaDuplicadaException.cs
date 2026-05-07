namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando ya existe una cuenta contable con el mismo código en el plan.
/// HTTP 409 - Conflict.
/// </summary>
public class CuentaDuplicadaException : Exception
{
    private const string MensajePorDefecto =
        "Ya existe una cuenta contable con el mismo código en el plan de cuentas.";

    public CuentaDuplicadaException()
        : base(MensajePorDefecto)
    {
    }

    public CuentaDuplicadaException(string mensaje)
        : base(mensaje)
    {
    }

    public CuentaDuplicadaException(Guid planCuentasId, string codigo)
        : base($"Ya existe una cuenta con código '{codigo}' en el plan '{planCuentasId}'.")
    {
    }
}
