using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Filters;

/// <summary>
/// Filtro global que asegura que ViewData["ClienteId"] y ViewData["ClienteNombre"]
/// estén disponibles en cualquier vista que opere dentro del contexto de un cliente,
/// sin necesidad de configurarlo manualmente en cada acción.
/// </summary>
public class ClienteContextFilter : IAsyncActionFilter
{
    private readonly ApiClient _apiClient;

    public ClienteContextFilter(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Intentar obtener clienteId de los argumentos enlazados de la acción
        Guid? clienteId = null;

        if (context.ActionArguments.TryGetValue("clienteId", out var argVal) && argVal is Guid argGuid)
            clienteId = argGuid;

        // 2. Fallback: ruta o query string
        if (!clienteId.HasValue)
        {
            var raw = context.RouteData.Values["clienteId"]?.ToString()
                   ?? context.HttpContext.Request.Query["clienteId"].FirstOrDefault();

            if (!string.IsNullOrEmpty(raw) && Guid.TryParse(raw, out var parsed))
                clienteId = parsed;
        }

        var executed = await next();

        // Solo actuar en vistas
        if (executed.Result is not ViewResult viewResult)
            return;

        var viewData = viewResult.ViewData;

        // 3. Preferir lo que la acción ya haya establecido en ViewData
        if (viewData["ClienteId"] is Guid vdGuid)
            clienteId = vdGuid;

        if (!clienteId.HasValue)
            return;

        // 4. Si ambos valores ya están presentes, no hacer nada
        if (viewData["ClienteId"] is Guid && !string.IsNullOrEmpty(viewData["ClienteNombre"] as string))
            return;

        viewData["ClienteId"] = clienteId.Value;

        // 5. Obtener el nombre del cliente si no está disponible
        if (string.IsNullOrEmpty(viewData["ClienteNombre"] as string))
        {
            var response = await _apiClient.GetAsync<ClienteListViewModel>($"api/clientes/{clienteId}");
            if (response.EsExitoso && response.Data is not null)
                viewData["ClienteNombre"] = response.Data.RazonSocial;
        }
    }
}
