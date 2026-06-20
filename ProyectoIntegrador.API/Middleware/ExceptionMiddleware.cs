using System.Net;
using System.Security.Claims;
using System.Text.Json;
using ProyectoIntegrador.Service.Exceptions;

namespace ProyectoIntegrador.API.Middleware;

/// <summary>
/// Intercepta todas las excepciones no controladas y las traduce
/// a respuestas HTTP con codigo y mensaje apropiados.
/// Loggea con nivel diferenciado segun gravedad: Warning para errores
/// de negocio esperados (4xx), Error para fallos inesperados (5xx).
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, mensaje) = ex switch
        {
            // 400 - Bad Request
            AsientoDesbalanceadoException e    => (HttpStatusCode.BadRequest, e.Message),
            AsientoNoBalanceadoException e     => (HttpStatusCode.BadRequest, e.Message),
            EjercicioCerradoException e        => (HttpStatusCode.BadRequest, e.Message),
            EjercicioSolapadoException e       => (HttpStatusCode.BadRequest, e.Message),
            CuentaNoImputableException e       => (HttpStatusCode.BadRequest, e.Message),
            CuentaJerarquiaInvalidaException e => (HttpStatusCode.BadRequest, e.Message),
            CuentaConMovimientosException e    => (HttpStatusCode.BadRequest, e.Message),
            ImportacionInvalidaException e     => (HttpStatusCode.BadRequest, e.Message),
            ValidacionException e              => (HttpStatusCode.BadRequest, e.Message),

            // 403 - Forbidden
            AccesoNoAutorizadoException e      => (HttpStatusCode.Forbidden, e.Message),

            // 404 - Not Found
            EntidadNoEncontradaException e     => (HttpStatusCode.NotFound, e.Message),

            // 409 - Conflict
            AsientoYaRevertidoException e      => (HttpStatusCode.Conflict, e.Message),
            DuplicadoException e               => (HttpStatusCode.Conflict, e.Message),
            CuentaDuplicadaException e         => (HttpStatusCode.Conflict, e.Message),

            // 500 - fallback generico
            _ => (HttpStatusCode.InternalServerError, "Ocurrio un error interno. Por favor intente nuevamente.")
        };

        var usuarioId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? "anonimo";
        var metodo = context.Request.Method;
        var ruta   = context.Request.Path.Value ?? string.Empty;
        var ip     = context.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        var codigo = (int)statusCode;

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex,
                "Error 500 | {Metodo} {Ruta} | Usuario: {UsuarioId} | IP: {IP} | Tipo: {TipoExcepcion} | Mensaje: {Mensaje}",
                metodo, ruta, usuarioId, ip, ex.GetType().Name, ex.Message);
        }
        else if (statusCode == HttpStatusCode.Forbidden)
        {
            _logger.LogWarning(
                "Acceso denegado {Codigo} | {Metodo} {Ruta} | Usuario: {UsuarioId} | IP: {IP} | Mensaje: {Mensaje}",
                codigo, metodo, ruta, usuarioId, ip, ex.Message);
        }
        else
        {
            _logger.LogWarning(
                "Excepcion de negocio {Codigo} | {Metodo} {Ruta} | Usuario: {UsuarioId} | Tipo: {TipoExcepcion} | Mensaje: {Mensaje}",
                codigo, metodo, ruta, usuarioId, ex.GetType().Name, ex.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = codigo;

        var respuesta = new
        {
            status = codigo,
            error = mensaje
        };

        var json = JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
