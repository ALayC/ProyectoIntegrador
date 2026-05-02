using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace ProyectoIntegrador.Test;

/// <summary>
/// Middleware que fija RemoteIpAddress = 127.0.0.1 en cada request.
/// Esto garantiza que el rate limiter use siempre la misma partition key
/// y acumule el contador correctamente dentro de un mismo test.
/// </summary>
internal sealed class FixedIpMiddleware
{
    private readonly RequestDelegate _next;

    public FixedIpMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        context.Connection.RemoteIpAddress = IPAddress.Loopback; // 127.0.0.1
        return _next(context);
    }
}

/// <summary>
/// IStartupFilter que inserta FixedIpMiddleware AL PRINCIPIO del pipeline,
/// ANTES de UseRateLimiter, para que el rate limiter ya vea la IP fija.
/// </summary>
internal sealed class FixedIpStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app =>
    {
        app.UseMiddleware<FixedIpMiddleware>();
        next(app);
    };
}

/// <summary>
/// Factory personalizada para los integration tests de rate limiting.
/// Registra FixedIpStartupFilter para que todos los requests del test
/// compartan la IP 127.0.0.1 y el contador del rate limiter se acumule.
///
/// SIN IClassFixture: cada [Fact] instancia la clase → nueva factory →
/// rate limiter en cero → aislamiento garantizado entre tests.
/// </summary>
internal sealed class RateLimitTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddTransient<IStartupFilter, FixedIpStartupFilter>();
        });
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Tests: POST /api/auth/login  →  límite: 10 intentos por IP cada 15 minutos
// ──────────────────────────────────────────────────────────────────────────────
public class LoginRateLimitingTests
{
    private readonly HttpClient _client;


    public LoginRateLimitingTests()
    {
        var factory = new RateLimitTestFactory();
        factory.Server.BaseAddress = new Uri("https://localhost");
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Envía 11 requests a POST /api/auth/login.
    /// Los primeros 10 pasan (pueden devolver cualquier código de negocio: 400, 404, etc.).
    /// El request número 11 debe ser rechazado con HTTP 429 por el rate limiter.
    /// </summary>
    [Fact]
    public async Task Login_AlSuperarElLimiteDe10Intentos_Devuelve429()
    {

        // Arrange
        var payload = new { email = "test@test.com", password = "password-invalido" };
        HttpResponseMessage? ultimaRespuesta = null;

        // Act: 11 requests (límite = 10)
        for (int i = 1; i <= 11; i++)
            ultimaRespuesta = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        Assert.NotNull(ultimaRespuesta);
        Assert.Equal(HttpStatusCode.TooManyRequests, ultimaRespuesta.StatusCode);
    }

    /// <summary>
    /// Verifica que la respuesta 429 incluye el header Retry-After con valor numérico positivo.
    /// </summary>
    [Fact]
    public async Task Login_AlSuperarElLimite_LaRespuestaIncluyeHeaderRetryAfter()
    {
        // Arrange
        var payload = new { email = "test@test.com", password = "password-invalido" };
        HttpResponseMessage? respuesta429 = null;

        // Act: enviar hasta obtener el primer 429
        for (int i = 1; i <= 11; i++)
        {
            var respuesta = await _client.PostAsJsonAsync("/api/auth/login", payload);
            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                respuesta429 = respuesta;
                break;
            }
        }

        // Assert
        Assert.NotNull(respuesta429);
        Assert.True(
           respuesta429.Headers.Contains("Retry-After"),
                  "La respuesta 429 debe incluir el header Retry-After.");

        var valor = respuesta429.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.True(
     int.TryParse(valor, out var segundos) && segundos > 0,
 $"Retry-After debe ser un número positivo, pero fue: '{valor}'.");
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Tests: POST /api/auth/register  →  límite: 5 intentos por IP cada 15 minutos
// ──────────────────────────────────────────────────────────────────────────────
public class RegisterRateLimitingTests
{
    private readonly HttpClient _client;

    public RegisterRateLimitingTests()
    {
        var factory = new RateLimitTestFactory();
        factory.Server.BaseAddress = new Uri("https://localhost");
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Envía 6 requests a POST /api/auth/register.
    /// Los primeros 5 pasan. El request número 6 debe ser rechazado con HTTP 429.
    /// </summary>
    [Fact]
    public async Task Register_AlSuperarElLimiteDe5Intentos_Devuelve429()
    {
        // Arrange
        var payload = new { email = "test-rate@test.com", password = "Password123!", nombreCompleto = "Test" };
        HttpResponseMessage? ultimaRespuesta = null;

        // Act: 6 requests (límite = 5)
        for (int i = 1; i <= 6; i++)
            ultimaRespuesta = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.NotNull(ultimaRespuesta);
        Assert.Equal(HttpStatusCode.TooManyRequests, ultimaRespuesta.StatusCode);
    }

    /// <summary>
    /// Verifica que los primeros 5 requests a /register NO son rechazados por rate limiting.
    /// Pueden fallar por negocio (409, 400), pero nunca con 429 dentro del límite.
    /// </summary>
    [Fact]
    public async Task Register_DentroDelLimiteDe5Intentos_NoDevuelve429()
    {
        // Arrange
        var payload = new { email = "dentro-limite@test.com", password = "Password123!", nombreCompleto = "Test" };

        // Act y Assert
        for (int i = 1; i <= 5; i++)
        {
            var respuesta = await _client.PostAsJsonAsync("/api/auth/register", payload);
            Assert.True(
         respuesta.StatusCode != HttpStatusCode.TooManyRequests,
                         $"Request #{i} no debería devolver 429, pero devolvió {(int)respuesta.StatusCode} {respuesta.StatusCode}.");
        }
    }
}
