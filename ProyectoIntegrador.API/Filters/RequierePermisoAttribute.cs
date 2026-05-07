namespace ProyectoIntegrador.API.Filters;

/// <summary>
/// Declara el permiso requerido (Modulo + Accion) para acceder a un endpoint.
/// Usado por PermisosActionFilter para verificar el permiso del rol del usuario autenticado.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequierePermisoAttribute : Attribute
{
    public string Modulo { get; }
    public string Accion { get; }

    public RequierePermisoAttribute(string modulo, string accion)
    {
        Modulo = modulo;
        Accion = accion;
    }
}
