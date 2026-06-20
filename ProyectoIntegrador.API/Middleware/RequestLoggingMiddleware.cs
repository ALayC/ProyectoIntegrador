using System.Diagnostics;
using System.Security.Claims;

namespace ProyectoIntegrador.API.Middleware;

/// <summary>
/// Loggea cada request HTTP con metodo, ruta, status code, duracion y usuario.
/// Nivel: Information para 2xx/3xx, Warning para 4xx, Error para 5xx.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        var usuarioId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? "anonimo";

        var metodo     = context.Request.Method;
        var ruta       = context.Request.Path.Value ?? string.Empty;
        var statusCode = context.Response.StatusCode;
        var duracionMs = sw.ElapsedMilliseconds;
        var ip         = context.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP {Metodo} {Ruta} -> {StatusCode} | {DuracionMs}ms | Usuario: {UsuarioId} | IP: {IP}",
                metodo, ruta, statusCode, duracionMs, usuarioId, ip);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP {Metodo} {Ruta} -> {StatusCode} | {DuracionMs}ms | Usuario: {UsuarioId} | IP: {IP}",
                metodo, ruta, statusCode, duracionMs, usuarioId, ip);
        }
        else
        {
            _logger.LogInformation(
                "HTTP {Metodo} {Ruta} -> {StatusCode} | {DuracionMs}ms | Usuario: {UsuarioId}",
                metodo, ruta, statusCode, duracionMs, usuarioId);
        }
    }
}
