using System.Net;
using System.Text.Json;
using ProyectoIntegrador.Service.Exceptions;

namespace ProyectoIntegrador.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

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

    private async Task ManejarExcepcionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, codigoError) = MapearExcepcion(exception);

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception,
             "Error interno no controlado en {Method} {Path}",
                context.Request.Method,
                       context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception,
        "Excepción de dominio ({CodigoError}) en {Method} {Path}: {Mensaje}",
               codigoError,
             context.Request.Method,
          context.Request.Path,
         exception.Message);
        }

        var respuesta = new ErrorResponse
        {
            Error = statusCode == (int)HttpStatusCode.InternalServerError
                  ? "Ocurrió un error interno en el servidor."
                   : exception.Message,
            Codigo = codigoError,
            Detalles = Array.Empty<string>()
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(respuesta, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string CodigoError) MapearExcepcion(Exception exception)
    {
        return exception switch
        {
            AsientoNoBalanceadoException => ((int)HttpStatusCode.BadRequest, "ASIENTO_NO_BALANCEADO"),
            EjercicioCerradoException => ((int)HttpStatusCode.BadRequest, "EJERCICIO_CERRADO"),
            CuentaNoImputableException => ((int)HttpStatusCode.BadRequest, "CUENTA_NO_IMPUTABLE"),
            EjercicioSolapadoException => ((int)HttpStatusCode.BadRequest, "EJERCICIO_SOLAPADO"),
            ImportacionInvalidaException => ((int)HttpStatusCode.BadRequest, "IMPORTACION_INVALIDA"),
            EntidadNoEncontradaException => ((int)HttpStatusCode.NotFound, "ENTIDAD_NO_ENCONTRADA"),
            AccesoNoAutorizadoException => ((int)HttpStatusCode.Forbidden, "ACCESO_NO_AUTORIZADO"),
            DuplicadoException => ((int)HttpStatusCode.Conflict, "DUPLICADO"),
            _ => ((int)HttpStatusCode.InternalServerError, "ERROR_INTERNO")
        };
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string[] Detalles { get; set; } = Array.Empty<string>();
    }
}
