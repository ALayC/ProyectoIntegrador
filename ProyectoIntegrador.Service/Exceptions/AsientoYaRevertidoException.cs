namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta revertir un asiento que ya fue revertido previamente.
/// HTTP 409 - Conflict.
/// </summary>
public class AsientoYaRevertidoException : Exception
{
    public AsientoYaRevertidoException(Guid asientoId)
        : base($"El asiento '{asientoId}' ya fue revertido y no puede volver a revertirse.")
    {
    }
}