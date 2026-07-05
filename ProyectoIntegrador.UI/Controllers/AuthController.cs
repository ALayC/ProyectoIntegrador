using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ProyectoIntegrador.UI.Controllers;

public class AuthController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApiClient apiClient, ILogger<AuthController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
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

        // Leer cookie de dispositivo confiable para pasarla a la API
        Request.Cookies.TryGetValue("trusted_device", out var tokenDispositivo);

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password,
            tokenDispositivo
        });

        if (!response.EsExitoso || response.Data is null)
        {
            var mensajeError = response.MensajeError ?? "Error al iniciar sesión.";
            ModelState.AddModelError(string.Empty, mensajeError);

            if (mensajeError.Contains("confirma tu email", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.EmailNoConfirmado = model.Email;
            }

            return View(model);
        }

        // Si requiere 2FA, redirigir a la página de verificación
        if (response.Data.Requires2FA && response.Data.TempToken is not null)
        {
            return RedirectToAction(nameof(Verify2FA), new { tempToken = response.Data.TempToken });
        }

        // Login directo (dispositivo confiable)
        HttpContext.Session.SetString("JwtToken", response.Data.Token!);
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

        TempData["AuthExito"] = "Cuenta creada exitosamente. Iniciá sesión.";
        return RedirectToAction(nameof(Login));
    }

    // ?? GET /Auth/Verify2FA ????????????????????????
    [HttpGet]
    public IActionResult Verify2FA(string tempToken)
    {
        if (string.IsNullOrWhiteSpace(tempToken))
            return RedirectToAction(nameof(Login));

        return View(new Verify2FAViewModel { TempToken = tempToken });
    }

    // ?? POST /Auth/Verify2FA ???????????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2FA(Verify2FAViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/verify-2fa", new
        {
            tempToken = model.TempToken,
            codigo = model.Codigo,
            recordarDispositivo = model.RecordarDispositivo
        });

        if (!response.EsExitoso || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Código inválido.");
            return View(model);
        }

        // Si el usuario marcó "recordar dispositivo", guardar cookie 7 días
        if (model.RecordarDispositivo && response.Data.TempToken is not null)
        {
            Response.Cookies.Append("trusted_device", response.Data.TempToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        HttpContext.Session.SetString("JwtToken", response.Data.Token!);
        await CrearCookieDeAutenticacion(response.Data);

        return RedirectToAction("Index", "Home");
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

        // Google también requiere 2FA
        if (response.Data.Requires2FA && response.Data.TempToken is not null)
        {
            return RedirectToAction(nameof(Verify2FA), new { tempToken = response.Data.TempToken });
        }

        HttpContext.Session.SetString("JwtToken", response.Data.Token!);
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
            new(ClaimTypes.Role, authData.Rol),
            new("JwtToken", authData.Token ?? string.Empty)
        };

        // Extraer el sub (userId) del JWT para guardarlo como claim
        var handler = new JwtSecurityTokenHandler();
        if (authData.Token is not null && handler.CanReadToken(authData.Token))
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

    // ?? GET /Auth/ForgotPassword ??????????????????
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // ?? POST /Auth/ForgotPassword ?????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiClient.PostAsync<object>("api/auth/forgot-password", new
        {
            email = model.Email
        });

        if (!response.EsExitoso)
        {
            // No revelar si el email existe o no por seguridad
            // Siempre mostrar mensaje de éxito
        }

        TempData["AuthExito"] = "Si tu email está registrado, recibirás instrucciones para restablecer tu contraseña.";
        return RedirectToAction(nameof(Login));
    }

    // ?? GET /Auth/ResetPassword ???????????????????
    [HttpGet]
    public IActionResult ResetPassword(string token)
    {


        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "❌ Token de restablecimiento inválido o no proporcionado.";
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    // ?? POST /Auth/ResetPassword ??????????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        // 🔥 LOG TEMPORAL PARA DEBUG
        Console.WriteLine($"[DEBUG POST] ========================================");
        Console.WriteLine($"[DEBUG POST] Token: '{model.Token}' (length: {model.Token?.Length ?? 0})");
        Console.WriteLine($"[DEBUG POST] NuevaContraseña: '{model.NuevaContraseña}' (length: {model.NuevaContraseña?.Length ?? 0})");
        Console.WriteLine($"[DEBUG POST] ConfirmarContraseña: '{model.ConfirmarContraseña}' (length: {model.ConfirmarContraseña?.Length ?? 0})");
        Console.WriteLine($"[DEBUG POST] ModelState.IsValid: {ModelState.IsValid}");
        Console.WriteLine($"[DEBUG POST] ModelState.ErrorCount: {ModelState.ErrorCount}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine($"[DEBUG POST] ❌ ERRORES DE VALIDACIÓN:");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Any())
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"[DEBUG POST]   - Campo '{key}': {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                            Console.WriteLine($"[DEBUG POST]     Exception: {error.Exception.Message}");
                        }
                    }
                }
            }
            Console.WriteLine($"[DEBUG POST] ========================================");
            return View(model);
        }

        Console.WriteLine($"[DEBUG POST] ✅ Validación OK, enviando a API...");

        var response = await _apiClient.PostAsync<AuthApiResponse>("api/auth/reset-password", new
        {
            token = model.Token,
            nuevaContraseña = model.NuevaContraseña,
            confirmarContraseña = model.ConfirmarContraseña
        });

        if (!response.EsExitoso || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al restablecer contraseña.");
            return View(model);
        }

        // Redirigir al login para que el usuario ingrese con sus nuevas credenciales
        TempData["Exito"] = "✅ Contraseña actualizada correctamente. Inicia sesión con tus nuevas credenciales.";
        return RedirectToAction("Login");
    }

    // 📧 GET /Auth/ConfirmEmail ????????????????????
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View(new ConfirmEmailViewModel
            {
                EsExito = false,
                Mensaje = "❌ Token de confirmación no proporcionado o inválido."
            });
        }

        try
        {
            // Llamar al API para confirmar el email
            var response = await _apiClient.PostAsync<AuthApiResponse>($"api/auth/confirm-email?token={token}", null);

            if (response.EsExitoso && response.Data != null)
            {
                return View(new ConfirmEmailViewModel
                {
                    EsExito = true,
                    Mensaje = "✅ Email confirmado exitosamente. Ya puedes iniciar sesión."
                });
            }

            return View(new ConfirmEmailViewModel
            {
                EsExito = false,
                Mensaje = response.MensajeError ?? "❌ Error al confirmar el email. El link puede haber expirado."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar email con token: {Token}", token);
            return View(new ConfirmEmailViewModel
            {
                EsExito = false,
                Mensaje = "❌ Ocurrió un error al confirmar tu email. Por favor, solicita un nuevo link de confirmación."
            });
        }
    }

    // 📧 GET /Auth/ResendConfirmation ?????????????
    [HttpGet]
    public IActionResult ResendConfirmation(string? email = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new ResendConfirmationViewModel 
        { 
            Email = email ?? string.Empty 
        });
    }

    // 📧 POST /Auth/ResendConfirmation ????????????
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiClient.PostAsync<object>("api/auth/resend-confirmation-email", new
            {
                email = model.Email
            });

            if (response.EsExitoso)
            {
                model.EsExito = true;
                model.Mensaje = "✅ Email de confirmación reenviado. Revisa tu bandeja de entrada.";
                return View(model);
            }

            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al reenviar el email.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar confirmación de email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Ocurrió un error al procesar tu solicitud.");
            return View(model);
        }
    }

    /// <summary>
    /// Modelo interno para deserializar la respuesta de la API de auth.
    /// </summary>
    private class AuthApiResponse
    {
        public string? Token { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Requires2FA { get; set; }
        public string? TempToken { get; set; }
    }
}


