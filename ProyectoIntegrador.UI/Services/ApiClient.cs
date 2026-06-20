using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ProyectoIntegrador.UI.Services;

/// <summary>
/// Servicio auxiliar que centraliza todas las llamadas HTTP a la API.
/// Agrega automáticamente el JWT de la sesión y maneja errores estándar.
/// </summary>
public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Realiza un GET y deserializa la respuesta.
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        var client = CrearClienteConToken();
        try
        {
            var response = await client.GetAsync(url);
            LogearRespuesta("GET", url, response.StatusCode);
            return await ProcesarRespuesta<T>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Error de comunicacion con la API | GET {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Realiza un POST con body JSON y deserializa la respuesta.
    /// </summary>
    public async Task<ApiResponse<T>> PostAsync<T>(string url, object data)
    {
        var client = CrearClienteConToken();
        var content = SerializarContenido(data);
        try
        {
            var response = await client.PostAsync(url, content);
            LogearRespuesta("POST", url, response.StatusCode);
            return await ProcesarRespuesta<T>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Error de comunicacion con la API | POST {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Realiza un PUT con body JSON y deserializa la respuesta.
    /// </summary>
    public async Task<ApiResponse<T>> PutAsync<T>(string url, object data)
    {
        var client = CrearClienteConToken();
        var content = SerializarContenido(data);
        try
        {
            var response = await client.PutAsync(url, content);
            LogearRespuesta("PUT", url, response.StatusCode);
            return await ProcesarRespuesta<T>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Error de comunicacion con la API | PUT {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Realiza un PATCH sin body (para acciones como desactivar).
    /// </summary>
    public async Task<ApiResponse<object>> PatchAsync(string url)
    {
        var client = CrearClienteConToken();
        var request = new HttpRequestMessage(HttpMethod.Patch, url);
        try
        {
            var response = await client.SendAsync(request);
            LogearRespuesta("PATCH", url, response.StatusCode);
            return await ProcesarRespuesta<object>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Error de comunicacion con la API | PATCH {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Realiza un DELETE sin body.
    /// </summary>
    public async Task<ApiResponse<object>> DeleteAsync(string url)
    {
        var client = CrearClienteConToken();
        try
        {
            var response = await client.DeleteAsync(url);
            LogearRespuesta("DELETE", url, response.StatusCode);
            return await ProcesarRespuesta<object>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Error de comunicacion con la API | DELETE {Url}", url);
            throw;
        }
    }

    // ??????????????????????????????????????????????
    // Métodos privados
    // ??????????????????????????????????????????????

    private HttpClient CrearClienteConToken()
    {
        var client = _httpClientFactory.CreateClient("API");
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private void LogearRespuesta(string metodo, string url, HttpStatusCode statusCode)
    {
        var codigo = (int)statusCode;
        if (codigo >= 500)
            _logger.LogError("API respondio {StatusCode} | {Metodo} {Url}", codigo, metodo, url);
        else if (codigo >= 400)
            _logger.LogWarning("API respondio {StatusCode} | {Metodo} {Url}", codigo, metodo, url);
    }

    private static StringContent SerializarContenido(object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<ApiResponse<T>> ProcesarRespuesta<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var data = string.IsNullOrWhiteSpace(body)
         ? default
                : JsonSerializer.Deserialize<T>(body, JsonOptions);

            return ApiResponse<T>.Ok(data);
        }

        // Extraer mensaje de error del cuerpo de la API
        var mensajeError = ExtraerMensajeError(body, response.StatusCode);

        return ApiResponse<T>.Error(mensajeError, response.StatusCode);
    }

    private static string ExtraerMensajeError(string body, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => "Su sesión ha expirado. Inicie sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tiene permisos para realizar esta operación.",
                HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado.",
                _ => "Ocurrió un error inesperado."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var errorProp))
            {
                return errorProp.GetString() ?? "Ocurrió un error inesperado.";
            }
            // Fallback: try "title" (ASP.NET validation errors)
            if (doc.RootElement.TryGetProperty("title", out var titleProp))
            {
                return titleProp.GetString() ?? "Ocurrió un error inesperado.";
            }
        }
        catch (JsonException)
        {
            // Body is not valid JSON
        }

        return "Ocurrió un error inesperado.";
    }
}

/// <summary>
/// Wrapper de respuesta de la API para manejar éxito y error de forma uniforme.
/// </summary>
public class ApiResponse<T>
{
    public bool EsExitoso { get; set; }
    public T? Data { get; set; }
    public string? MensajeError { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Indica si la API respondió 401 (sesión expirada).
    /// </summary>
    public bool EsNoAutorizado => StatusCode == HttpStatusCode.Unauthorized;

    public static ApiResponse<T> Ok(T? data) => new()
    {
        EsExitoso = true,
        Data = data,
        StatusCode = HttpStatusCode.OK
    };

    public static ApiResponse<T> Error(string mensaje, HttpStatusCode statusCode) => new()
    {
        EsExitoso = false,
        MensajeError = mensaje,
        StatusCode = statusCode
    };
}
