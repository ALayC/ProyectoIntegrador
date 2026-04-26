using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

public class AuthController : Controller
{
    private readonly ApiClient _apiClient;

    public AuthController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // ?? GET /Auth/Login ???????????????????????????
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // ?? POST /Auth/Login ??????????????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });

        if (!response.EsExitoso || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al iniciar sesión.");
            return View(model);
        }

        // Guardar JWT en sesión
        HttpContext.Session.SetString("JwtToken", response.Data.Token);

        // Crear cookie de autenticación con claims
        await CrearCookieDeAutenticacion(response.Data);

        return RedirectToAction("Index", "Home");
    }

    // ?? GET /Auth/Register ????????????????????????
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // ?? POST /Auth/Register ???????????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/register", new
        {
            email = model.Email,
            password = model.Password,
            nombreCompleto = model.NombreCompleto
        });

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al registrarse.");
            return View(model);
        }

        TempData["Exito"] = "Cuenta creada exitosamente. Iniciá sesión.";
        return RedirectToAction(nameof(Login));
    }

    // ?? POST /Auth/Logout ?????????????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Intentar notificar a la API (si falla, igualmente limpiar la sesión local)
        await _apiClient.PostAsync<object>("api/auth/logout", new { });

        // Limpiar sesión y cookie
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    // ?? GET /Auth/LoginGoogle ??????????????????????
    [HttpGet]
    public IActionResult LoginGoogle()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal is null)
        {
            ModelState.AddModelError(string.Empty, "Error al autenticar con Google.");
            return View("Login");
        }

        // Log de debug
        var tokens = result.Properties?.GetTokens()?.ToList() ?? new();
        foreach (var t in tokens)
        {
            Console.WriteLine($"[GoogleCallback] Token: {t.Name} = {(string.IsNullOrEmpty(t.Value) ? "(vacío)" : t.Value[..Math.Min(20, t.Value.Length)] + "...")}");
        }

        var idToken = result.Properties?.GetTokenValue("id_token");
        var accessToken = result.Properties?.GetTokenValue("access_token");

        Console.WriteLine($"[UI → API] idToken length: {idToken?.Length ?? 0}");
        Console.WriteLine($"[UI → API] accessToken length: {accessToken?.Length ?? 0}");

        if (string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
        {
            ModelState.AddModelError(string.Empty, "No se pudo obtener el token de Google.");
            return View("Login");
        }

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/google-login", new
        {
            IdToken = idToken,
            AccessToken = accessToken
        });

        if (!response.EsExitoso || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al iniciar sesión con Google.");
            return View("Login");
        }

        HttpContext.Session.SetString("JwtToken", response.Data.Token);
        await CrearCookieDeAutenticacion(response.Data);

        return RedirectToAction("Index", "Home");
    }

    // ??????????????????????????????????????????????
    // Helpers privados
    // ??????????????????????????????????????????????
    private async Task CrearCookieDeAutenticacion(AuthApiResponse authData)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, authData.NombreCompleto),
            new(ClaimTypes.Email, authData.Email),
            new(ClaimTypes.Role, authData.Rol)
        };

        // Extraer el sub (userId) del JWT para guardarlo como claim
        var handler = new JwtSecurityTokenHandler();
        if (handler.CanReadToken(authData.Token))
        {
            var jwt = handler.ReadJwtToken(authData.Token);
            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subClaim != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            });
    }

    /// <summary>
    /// Modelo interno para deserializar la respuesta de la API de auth.
    /// </summary>
    private class AuthApiResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}


