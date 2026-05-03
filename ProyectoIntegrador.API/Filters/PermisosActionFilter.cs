using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Exceptions;

namespace ProyectoIntegrador.API.Filters;

/// <summary>
/// Action filter que verifica si el rol del usuario autenticado tiene el permiso
/// declarado mediante [RequierePermiso("Modulo", "Accion")] en el endpoint.
/// Si el permiso no existe ? lanza AccesoNoAutorizadoException ? ExceptionMiddleware ? HTTP 403.
/// </summary>
public sealed class PermisosActionFilter : IAsyncActionFilter
{
  private readonly IRolRepository _rolRepository;

    public PermisosActionFilter(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
  }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
     // Buscar el atributo en el endpoint actual
      var atributo = context.ActionDescriptor
   .EndpointMetadata
   .OfType<RequierePermisoAttribute>()
          .FirstOrDefault();

        // Si no tiene atributo, no hay validación de permiso adicional
        if (atributo is null)
   {
      await next();
       return;
    }

      // Obtener el RolId del claim del JWT
   var rolIdClaim = context.HttpContext.User.FindFirst("rolId")?.Value;
        if (string.IsNullOrEmpty(rolIdClaim) || !Guid.TryParse(rolIdClaim, out var rolId))
 throw new AccesoNoAutorizadoException("No se pudo determinar el rol del usuario.");

 // Consultar los permisos del rol
    var permisos = await _rolRepository.ObtenerPermisos(rolId);

    // Verificar si el rol tiene el permiso requerido
        var tienePermiso = permisos.Any(p =>
    p.Modulo == atributo.Modulo &&
  p.Accion == atributo.Accion);

if (!tienePermiso)
            throw new AccesoNoAutorizadoException(
   $"No tiene permiso para realizar la acción '{atributo.Accion}' en el módulo '{atributo.Modulo}'.");

    await next();
    }
}
