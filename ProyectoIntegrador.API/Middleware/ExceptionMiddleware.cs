using System.Net;
using System.Text.Json;
using ProyectoIntegrador.Service.Exceptions;

namespace ProyectoIntegrador.API.Middleware;

/// <summary>
/// Intercepta todas las excepciones no controladas y las traduce
/// a respuestas HTTP con código y mensaje apropiados.
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
            _logger.LogError(ex, "Excepción no controlada: {Mensaje}", ex.Message);
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private static async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, mensaje) = ex switch
        {
            // 400 – Bad Request
            AsientoDesbalanceadoException e    => (HttpStatusCode.BadRequest, e.Message),
            AsientoNoBalanceadoException e     => (HttpStatusCode.BadRequest, e.Message),
            EjercicioCerradoException e        => (HttpStatusCode.BadRequest, e.Message),
            EjercicioSolapadoException e       => (HttpStatusCode.BadRequest, e.Message),
            CuentaNoImputableException e       => (HttpStatusCode.BadRequest, e.Message),
            CuentaJerarquiaInvalidaException e => (HttpStatusCode.BadRequest, e.Message),
            CuentaConMovimientosException e    => (HttpStatusCode.BadRequest, e.Message),
            ImportacionInvalidaException e     => (HttpStatusCode.BadRequest, e.Message),
            ValidacionException e              => (HttpStatusCode.BadRequest, e.Message),

            // 403 – Forbidden
            AccesoNoAutorizadoException e      => (HttpStatusCode.Forbidden, e.Message),

            // 404 – Not Found
            EntidadNoEncontradaException e     => (HttpStatusCode.NotFound, e.Message),

            // 409 – Conflict
            AsientoYaRevertidoException e      => (HttpStatusCode.Conflict, e.Message),
            DuplicadoException e               => (HttpStatusCode.Conflict, e.Message),
            CuentaDuplicadaException e         => (HttpStatusCode.Conflict, e.Message),

            // 500 – fallback genérico (no expone detalles internos)
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error interno. Por favor intente nuevamente.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var respuesta = new
        {
            status = (int)statusCode,
            error = mensaje
        };

        var json = JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
