namespace ProyectoIntegrador.Service.Exceptions;

public class ComprobanteDuplicadoException : DuplicadoException
{
    public ComprobanteDuplicadoException(string numero, string rut, DateOnly fecha)
        : base("Comprobante", $"Número {numero}, RUT {rut}, Fecha {fecha:dd/MM/yyyy}")
    {
    }
}
