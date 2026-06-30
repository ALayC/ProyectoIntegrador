using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProyectoIntegrador.API.Filters;
using ProyectoIntegrador.API.Middleware;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Repositories.Implementations;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

// ──────────────────────────────────────────────
// Builder
// ──────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ── Application Insights ─────────────────────
var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// ── Entity Framework Core + SQL Server ────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("ProyectoIntegrador.Data")));

// ── Autenticación JWT Bearer ──────────────────
var jwtSecretKey = configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("La clave secreta JWT no está configurada en appsettings.json (Jwt:SecretKey).");
var jwtIssuer = configuration["Jwt:Issuer"] ?? "ProyectoIntegrador.API";
var jwtAudience = configuration["Jwt:Audience"] ?? "ProyectoIntegrador.UI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var repo = context.HttpContext.RequestServices
                .GetRequiredService<ITokenRevocadoRepository>();
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader["Bearer ".Length..].Trim()
                : string.Empty;

            if (!string.IsNullOrEmpty(token) && await repo.EstaRevocado(token))
            {
                context.Fail("Token revocado.");
            }
        }
    };
});

builder.Services.AddAuthorization();

// ── Opciones JWT para la capa Service ─────────
builder.Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

// ── CORS ──────────────────────────────────────
var origenesPermitidos = configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
    ?? ["https://localhost:7001", "http://localhost:5001"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirUI", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── Rate Limiting ─────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Callback global: agrega el header Retry-After en toda respuesta 429
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Demasiadas solicitudes. Intente nuevamente más tarde.\",\"codigo\":\"RATE_LIMIT_EXCEDIDO\",\"detalles\":[]}",
            cancellationToken);
    };

    // Limitador GLOBAL: 200 requests por minuto por usuario autenticado (o por IP como fallback).
    // Se aplica a TODA request sin pisar las políticas declaradas vía [EnableRateLimiting].
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var usuarioId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst("sub")?.Value;

        var partitionKey = !string.IsNullOrEmpty(usuarioId)
            ? $"user:{usuarioId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "desconocido"}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Política de login: 10 intentos cada 15 minutos por IP
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Política de registro: 5 intentos cada 15 minutos por IP
    options.AddPolicy("register", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ── Inyección de dependencias: Repositorios ───
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITokenRevocadoRepository, TokenRevocadoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPlanDeCuentasRepository, PlanDeCuentasRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<ICuentaContableRepository, CuentaContableRepository>();
builder.Services.AddScoped<IEjercicioContableRepository, EjercicioContableRepository>();
builder.Services.AddScoped<IAsientoContableRepository, AsientoContableRepository>();
builder.Services.AddScoped<ISaldoCuentaRepository, SaldoCuentaRepository>();
builder.Services.AddScoped<ILineaAsientoRepository, LineaAsientoRepository>();
builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
// Los demás repositorios se irán activando a medida que se creen las implementaciones
// builder.Services.AddScoped<IImportacionRepository, ImportacionRepository>();
// builder.Services.AddScoped<ICentroDeCostoRepository, CentroDeCostoRepository>();
// builder.Services.AddScoped<ITipoDeCambioRepository, TipoDeCambioRepository>();

// ── Opciones de configuración: Email ────────────
builder.Services.Configure<EmailOptions>(configuration.GetSection("Email"));

// ── Opciones de configuración: UI ────────────────
builder.Services.Configure<UIOptions>(configuration.GetSection("UI"));

// ── Inyección de dependencias: Servicios ──────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICuentaContableService, CuentaContableService>();
builder.Services.AddScoped<IEjercicioContableService, EjercicioContableService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IAsientoContableService, AsientoContableService>();
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<ILibroMayorService, LibroMayorService>();
builder.Services.AddScoped<ILineaAsientoRepository, LineaAsientoRepository>();
builder.Services.AddScoped<IEstadoResultadosService, EstadoResultadosService>();
builder.Services.AddScoped<IBalanceGeneralService, BalanceGeneralService>();
builder.Services.AddScoped<ILiquidacionIvaService, LiquidacionIvaService>();

// ── Filtro global de permisos ─────────────────
builder.Services.AddScoped<PermisosActionFilter>();

// ── Controllers ───────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<PermisosActionFilter>();
});

// ── Swagger (solo desarrollo) ─────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProyectoIntegrador API",
        Version = "v1",
        Description = "API del sistema contable para estudio contable en Uruguay"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ──────────────────────────────────────────────
// App (pipeline HTTP)
// ──────────────────────────────────────────────

var app = builder.Build();

// 1. Middleware global de manejo de excepciones (primero en el pipeline)
app.UseMiddleware<ExceptionMiddleware>();

// 2. Logging de requests (inmediatamente después del exception middleware)
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Swagger (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. HTTPS
app.UseHttpsRedirection();

// 4. Routing (debe ir ANTES de UseRateLimiter para que las políticas
//    aplicadas vía [EnableRateLimiting] a nivel de endpoint funcionen)
app.UseRouting();

// 5. CORS (después de UseRouting, antes de Authentication/Authorization)
app.UseCors("PermitirUI");

// 6. Rate Limiting (después de UseRouting para que conozca el endpoint destino)
app.UseRateLimiter();

// 7. Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 8. Endpoints (sin RequireRateLimiting; el GlobalLimiter ya cubre todos los endpoints)
app.MapControllers();

app.Run();

// Necesario para que WebApplicationFactory pueda referenciar esta clase desde los integration tests
public partial class Program { }