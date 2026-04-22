using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProyectoIntegrador.API.Middleware;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Repositories.Implementations;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Implementations;
using ProyectoIntegrador.Service.Interfaces;

// ??????????????????????????????????????????????
// Builder
// ??????????????????????????????????????????????

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ?? Entity Framework Core + SQL Server ????????
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("ProyectoIntegrador.Data")));

// ?? Autenticación JWT Bearer ??????????????????
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
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

// ?? Opciones JWT para la capa Service ?????????
builder.Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

// ?? CORS ??????????????????????????????????????
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

// ?? Rate Limiting ?????????????????????????????
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política global: 200 requests por minuto por IP
    options.AddPolicy("global", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
    factory: _ => new FixedWindowRateLimiterOptions
            {
            PermitLimit = 200,
   Window = TimeSpan.FromMinutes(1),
           QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
   QueueLimit = 0
    }));

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
});

// ?? Inyección de dependencias: Repositorios ???
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITokenRevocadoRepository, TokenRevocadoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPlanDeCuentasRepository, PlanDeCuentasRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
// Los demás repositorios se irán activando a medida que se creen las implementaciones:
// builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
// builder.Services.AddScoped<ICuentaContableRepository, CuentaContableRepository>();
// builder.Services.AddScoped<IEjercicioContableRepository, EjercicioContableRepository>();
// builder.Services.AddScoped<IAsientoContableRepository, AsientoContableRepository>();
// builder.Services.AddScoped<ILineaAsientoRepository, LineaAsientoRepository>();
// builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
// builder.Services.AddScoped<IImportacionRepository, ImportacionRepository>();
// builder.Services.AddScoped<ISaldoCuentaRepository, SaldoCuentaRepository>();
// builder.Services.AddScoped<ICentroDeCostoRepository, CentroDeCostoRepository>();
// builder.Services.AddScoped<ITipoDeCambioRepository, TipoDeCambioRepository>();

// ?? Inyección de dependencias: Servicios ??????
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
// Los demás servicios se irán activando a medida que se creen las implementaciones:
// builder.Services.AddScoped<IAsientoService, AsientoService>();
// builder.Services.AddScoped<IReporteService, ReporteService>();
// builder.Services.AddScoped<IImportacionService, ImportacionService>();
// builder.Services.AddScoped<ICierreEjercicioService, CierreEjercicioService>();

// ?? Controllers ???????????????????????????????
builder.Services.AddControllers();

// ?? Swagger (solo desarrollo) ?????????????????
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

// ??????????????????????????????????????????????
// App (pipeline HTTP)
// ??????????????????????????????????????????????

var app = builder.Build();

// 1. Middleware global de manejo de excepciones (primero en el pipeline)
app.UseMiddleware<ExceptionMiddleware>();

// 2. Swagger (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. HTTPS
app.UseHttpsRedirection();

// 4. CORS (debe ir antes de Authentication/Authorization)
app.UseCors("PermitirUI");

// 5. Rate Limiting
app.UseRateLimiter();

// 6. Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 7. Endpoints
app.MapControllers().RequireRateLimiting("global");

app.Run();
