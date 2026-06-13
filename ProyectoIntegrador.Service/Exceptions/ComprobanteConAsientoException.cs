namespace ProyectoIntegrador.Service.Exceptions;

public class ComprobanteConAsientoException : ValidacionException
{
    public ComprobanteConAsientoException(Guid comprobanteId)
        : base($"El comprobante {comprobanteId} ya está asociado a un asiento contable y no puede modificarse o anularse.")
    {
    }
}
