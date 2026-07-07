using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.CookiePolicy;
using ProyectoIntegrador.UI.Services;
using QuestPDF.Infrastructure;
using System.Net;
using System.Net.Sockets;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 🔹 Application Insights ─────────────────────
var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// 🔹 MVC con Views ─────────────────────────────
builder.Services.AddScoped<ProyectoIntegrador.UI.Filters.ClienteContextFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<ProyectoIntegrador.UI.Filters.ClienteContextFilter>();
});

// 🔹 HttpClient nombrado "API" ─────────────────
var apiBaseUrl = configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl no está configurada en appsettings.json.");

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// 🔹 Cookie policy global ──────────────────────
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;

    options.OnAppendCookie = cookieContext =>
    {
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
        cookieContext.CookieOptions.Secure = true;
    };

    options.OnDeleteCookie = cookieContext =>
    {
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
        cookieContext.CookieOptions.Secure = true;
    };
});

// 🔹 Sesión (para almacenar JWT) ───────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ProyectoIntegrador.Session";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// 🔹 Autenticación ─────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Error/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
.AddGoogle(options =>
{
    options.ClientId = configuration["Google:ClientId"]!;
    options.ClientSecret = configuration["Google:ClientSecret"]!;
    options.CallbackPath = "/signin-google";
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.HttpOnly = true;

    // 👇 Forzar re-consent para que Google devuelva id_token
    options.AccessType = "offline";
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        var url = context.RedirectUri + "&prompt=consent";
        context.Response.Redirect(url);
        return Task.CompletedTask;
    };

    options.Backchannel = new HttpClient(CreateIPv4Handler())
    {
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders =
        {
            { "User-Agent", "Microsoft ASP.NET Core OAuth handler" }
        }
    };
});

// 🔹 HttpContextAccessor ───────────────────────
builder.Services.AddHttpContextAccessor();

// 🔹 ApiClient (servicio auxiliar) ─────────────
builder.Services.AddScoped<ProyectoIntegrador.UI.Services.ApiClient>();
builder.Services.AddScoped<ProyectoIntegrador.UI.Services.AiService>();
builder.Services.AddScoped<ProyectoIntegrador.UI.Services.ILibroMayorExcelService,
                            ProyectoIntegrador.UI.Services.LibroMayorExcelService>();
builder.Services.AddScoped<ILibroMayorPdfService, LibroMayorPdfService>();
builder.Services.AddScoped<IEstadoResultadosExcelService, EstadoResultadosExcelService>();
builder.Services.AddScoped<IEstadoResultadosPdfService, EstadoResultadosPdfService>();
builder.Services.AddScoped<ProyectoIntegrador.UI.Services.IImportacionExcelService,
                            ProyectoIntegrador.UI.Services.ImportacionExcelService>();

// 🔹 HttpClient nombrado para BCU SOAP ─────────
builder.Services.AddHttpClient("BCU", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "ProyectoIntegrador/1.0");
});

var app = builder.Build();

// 🔹 Pipeline HTTP ─────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true &&
        string.IsNullOrEmpty(context.Session.GetString("JwtToken")))
    {
        var token = context.User.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            context.Session.SetString("JwtToken", token);
        }
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// 🔹 Handler que fuerza IPv4 ───────────────────
static SocketsHttpHandler CreateIPv4Handler() => new()
{
    ConnectCallback = async (context, cancellationToken) =>
    {
        var entries = await Dns.GetHostAddressesAsync(
            context.DnsEndPoint.Host,
            AddressFamily.InterNetwork,
            cancellationToken);

        if (entries.Length == 0)
            throw new InvalidOperationException(
                $"No se encontró dirección IPv4 para {context.DnsEndPoint.Host}");

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            await socket.ConnectAsync(entries[0], context.DnsEndPoint.Port, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
};