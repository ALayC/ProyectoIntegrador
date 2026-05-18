namespace ProyectoIntegrador.Service.Exceptions;

/// <summary>
/// Se lanza cuando se intenta crear un recurso que ya existe (RUT, email, código, etc.).
/// HTTP 409 - Conflict.
/// </summary>
public class DuplicadoException : Exception
{
    public DuplicadoException(string mensaje) : base(mensaje) { }

    public DuplicadoException(string campo, string valor)
        : base($"Ya existe un registro con {campo}: '{valor}'.") { }
}
