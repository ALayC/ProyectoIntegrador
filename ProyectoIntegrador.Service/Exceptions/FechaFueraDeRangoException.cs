namespace ProyectoIntegrador.Service.Exceptions;

public class FechaFueraDeRangoException : ValidacionException
{
    public FechaFueraDeRangoException(DateOnly fecha)
        : base($"La fecha '{fecha:dd/MM/yyyy}' está fuera de rango permitido.")
    {
    }
}
