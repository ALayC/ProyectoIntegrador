using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoIntegrador.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly ITokenRevocadoRepository _tokenRevocadoRepository;
    private readonly IDispositivoConfiableRepository _dispositivoConfiableRepository;
    private readonly IInvitacionAuxiliarRepository _invitacionRepository;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;
    private readonly UIOptions _uiOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        ITokenRevocadoRepository tokenRevocadoRepository,
        IDispositivoConfiableRepository dispositivoConfiableRepository,
        IInvitacionAuxiliarRepository invitacionRepository,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions,
        IOptions<UIOptions> uiOptions,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _tokenRevocadoRepository = tokenRevocadoRepository;
        _dispositivoConfiableRepository = dispositivoConfiableRepository;
        _invitacionRepository = invitacionRepository;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
        _uiOptions = uiOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Registrar(RegistroDto registroDto)
    {
        // Validar email único
        var existeEmail = await _usuarioRepository.ExisteEmail(registroDto.Email);
        if (existeEmail)
        {
            _logger.LogWarning("Intento de registro con email duplicado: {Email}", registroDto.Email);
            throw new DuplicadoException("email", registroDto.Email);
        }

        // Detectar si existe una invitación pendiente para este email
        var invitacion = await _invitacionRepository.ObtenerPendientePorEmail(registroDto.Email);
        var esAuxiliar = invitacion is not null;

        var rolId = esAuxiliar ? SeedData.RolAuxiliarId : SeedData.RolContadorId;
        var rolContador = await _rolRepository.ObtenerPorId(rolId)
            ?? throw new EntidadNoEncontradaException("Rol", rolId);

        // Generar token de confirmaci
        var tokenConfirmacion = Guid.NewGuid().ToString("N"); // Sin guiones
        var fechaExpiracion = DateTime.UtcNow.AddHours(24);

        // Crear usuario
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = registroDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registroDto.Password, workFactor: 12),
            NombreCompleto = registroDto.NombreCompleto,
            ProveedorAuth = "Local",
            Estado = "Activo",
            RolId = rolContador.Id,
            ContadorId = esAuxiliar ? invitacion!.ContadorId : null,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmado = false, // Email no confirmado aún
            TokenConfirmacionEmail = tokenConfirmacion,
            FechaExpiracionTokenConfirmacion = fechaExpiracion
        };

        await _usuarioRepository.Guardar(usuario);

        if (esAuxiliar)
        {
            invitacion!.Estado = "Aceptada";
            await _invitacionRepository.Actualizar(invitacion);
        }

        // Enviar email de confirmaci
        try
        {
            var baseUrl = _uiOptions.BaseUrl;
            _ = _emailService.EnviarConfirmacionEmailAsync(usuario.Email, tokenConfirmacion, baseUrl);

            // ?? LOG TEMPORAL PARA TESTING (eliminar en producción)
            var linkConfirmacion = $"{baseUrl}/Auth/ConfirmEmail?token={tokenConfirmacion}";
            _logger.LogWarning("?? [TESTING] Link de confirmación: {Link}", linkConfirmacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de confirmación a: {Email}", usuario.Email);
            // No lanzar excepción: el usuario se registró, pero el email falló
        }

        _logger.LogInformation("? Usuario registrado correctamente: {Email} | Rol: {Rol} | EmailPendiente: No confirmado", usuario.Email, rolContador.Nombre);

        // Devolver respuesta: usuario registrado pero email no confirmado
        return new AuthResponseDto
        {
            Token = null, // No generar token aún (email no confirmado)
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = rolContador.Nombre
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        // Buscar usuario por email
        var usuario = await _usuarioRepository.ObtenerPorEmail(loginDto.Email);
        if (usuario is null)
        {
            _logger.LogWarning("Intento de login fallido - usuario no encontrado: {Email}", loginDto.Email);
            throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");
        }

        // Verificar que tenga password (podría ser usuario Google)
        if (string.IsNullOrEmpty(usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de login local para usuario con auth externa: {Email}", loginDto.Email);
            throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");
        }

        // Verificar password con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de login con credenciales incorrectas: {Email}", loginDto.Email);
            throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");
        }

        // Verificar que el usuario esté activo
        if (usuario.Estado != "Activo")
        {
            _logger.LogWarning("Intento de login con cuenta inactiva: {Email}", loginDto.Email);
            throw new AccesoNoAutorizadoException("La cuenta de usuario se encuentra inactiva.");
        }

        // Verificar que el email esté confirmado
        if (!usuario.EmailConfirmado)
        {
            _logger.LogWarning("Intento de login con email no confirmado: {Email}", loginDto.Email);
            throw new ValidacionException("Por favor, confirma tu email antes de iniciar sesión. Revisa tu bandeja de entrada.");
        }

        // Verificar si el dispositivo ya es confiable (cookie de 7 días)
        if (!string.IsNullOrEmpty(loginDto.TokenDispositivo))
        {
            var dispositivo = await _dispositivoConfiableRepository.ObtenerPorToken(loginDto.TokenDispositivo);
            if (dispositivo is not null && dispositivo.UsuarioId == usuario.Id && dispositivo.FechaExpiracion > DateTime.UtcNow)
            {
                _logger.LogInformation("Login con dispositivo confiable: {Email}", usuario.Email);
                var tokenDirecto = GenerarToken(usuario, usuario.Rol.Nombre);
                return new AuthResponseDto
                {
                    Token = tokenDirecto,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Rol = usuario.Rol.Nombre
                };
            }
        }

        // Generar y enviar código 2FA
        var codigo = GenerarCodigo2FA();
        usuario.Codigo2FA = codigo;
        usuario.FechaExpiracion2FA = DateTime.UtcNow.AddMinutes(5);
        await _usuarioRepository.Actualizar(usuario);

        try
        {
            await _emailService.Enviar2FaCodeAsync(usuario.Email, codigo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar código 2FA a: {Email}", usuario.Email);
        }

        // Generar temp token (JWT de 5 min con claim especial, sin rol real)
        var tempToken = GenerarTempToken2FA(usuario);

        _logger.LogInformation("2FA iniciado para: {Email}", usuario.Email);

        return new AuthResponseDto
        {
            Token = null,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = string.Empty,
            Requires2FA = true,
            TempToken = tempToken
        };
    }

    public async Task Logout(Guid usuarioId, string token)
    {
        // Leer la expiración del token para registrarla
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var tokenRevocado = new TokenRevocado
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Token = token,
            ExpiraEn = jwtToken.ValidTo
        };

        await _tokenRevocadoRepository.Guardar(tokenRevocado);
        _logger.LogInformation("Logout exitoso: UsuarioId {UsuarioId}", usuarioId);
    }

    public async Task<AuthResponseDto> ObtenerUsuarioActual(Guid id)
    {
        var usuario = await _usuarioRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Usuario", id);

        return new AuthResponseDto
        {
            Token = string.Empty, // No se regenera token en este endpoint
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }

    /// <summary>
    /// Confirma el email del usuario usando el token generado al registrarse.
    /// </summary>
    public async Task<AuthResponseDto> ConfirmarEmailAsync(string token)
    {
        // Buscar usuario por token
        var usuario = await _usuarioRepository.ObtenerPorTokenConfirmacion(token)
            ?? throw new EntidadNoEncontradaException("Token de confirmación inválido o expirado.");

        // Verificar que el token no haya expirado
        if (usuario.FechaExpiracionTokenConfirmacion < DateTime.UtcNow)
        {
            _logger.LogWarning("Intento de confirmar email con token expirado: {Email}", usuario.Email);
            throw new ValidacionException("? El link de confirmación ha expirado. Por favor, solicita uno nuevo.");
        }

        // Marcar email como confirmado
        usuario.EmailConfirmado = true;
        usuario.TokenConfirmacionEmail = null;
        usuario.FechaExpiracionTokenConfirmacion = null;

        await _usuarioRepository.Actualizar(usuario);

        _logger.LogInformation("? Email confirmado exitosamente: {Email}", usuario.Email);

        // Generar JWT después de confirmar email
        var token_jwt = GenerarToken(usuario, usuario.Rol.Nombre);

        return new AuthResponseDto
        {
            Token = token_jwt,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }

    /// <summary>
    /// Reenvía email de confirmación si el anterior expiró.
    /// </summary>
    public async Task ReenviarConfirmacionEmailAsync(string email, string baseUrl)
    {
        var usuario = await _usuarioRepository.ObtenerPorEmail(email)
            ?? throw new ValidacionException("? El email ingresado no está registrado.");

        if (usuario.EmailConfirmado)
        {
            throw new ValidacionException("? Este email ya está confirmado. Puedes iniciar sesión.");
        }

        // Generar nuevo token (24 horas)
        var nuevoToken = Guid.NewGuid().ToString("N");
        usuario.TokenConfirmacionEmail = nuevoToken;
        usuario.FechaExpiracionTokenConfirmacion = DateTime.UtcNow.AddHours(24);

        await _usuarioRepository.Actualizar(usuario);

        // Enviar email
        try
        {
            await _emailService.EnviarConfirmacionEmailAsync(usuario.Email, nuevoToken, baseUrl);
            _logger.LogInformation("? Email de confirmación reenviado a: {Email}", usuario.Email);

            // ?? LOG TEMPORAL PARA TESTING (eliminar en producción)
            var linkConfirmacion = $"{baseUrl}/Auth/ConfirmEmail?token={nuevoToken}";
            _logger.LogWarning("?? [TESTING] Link de confirmación reenviado: {Link}", linkConfirmacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar email de confirmación a: {Email}", usuario.Email);
            throw;
        }
    }

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña.
    /// Envía email con link de restablecimiento.
    /// </summary>
    public async Task SolicitarRestablecimientoContraseñaAsync(string email, string baseUrl)
    {
        var usuario = await _usuarioRepository.ObtenerPorEmail(email)
            ?? throw new ValidacionException("? El email ingresado no está registrado.");

        // Solo usuarios con password local pueden hacer reset
        if (string.IsNullOrEmpty(usuario.PasswordHash) || usuario.ProveedorAuth != "Local")
        {
            throw new ValidacionException("? Este usuario no tiene contraseña local. Usa Google Login.");
        }

        // Generar token de restablecimiento (1 hora de validez)
        var tokenReset = Guid.NewGuid().ToString("N");
        usuario.TokenRestablecimiento = tokenReset;
        usuario.FechaExpiracionTokenRestablecimiento = DateTime.UtcNow.AddHours(1);

        await _usuarioRepository.Actualizar(usuario);

        // Enviar email
        try
        {
            await _emailService.EnviarRestablecimientoContraseñaAsync(usuario.Email, tokenReset, baseUrl);
            _logger.LogInformation("?? Email de restablecimiento enviado a: {Email}", usuario.Email);

            // ?? LOG TEMPORAL PARA TESTING (eliminar en producción)
            var linkReset = $"{baseUrl}/Auth/ResetPassword?token={tokenReset}";
            _logger.LogWarning("?? [TESTING] Link de reset: {Link}", linkReset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de restablecimiento a: {Email}", usuario.Email);
            throw;
        }
    }

    /// <summary>
    /// Restablece la contraseña del usuario usando el token enviado por email.
    /// </summary>
    public async Task<AuthResponseDto> RestablecerContraseñaAsync(string token, string nuevaContraseña)
    {
        // Validar contraseña
        if (string.IsNullOrWhiteSpace(nuevaContraseña) || nuevaContraseña.Length < 8)
        {
            throw new ValidacionException("? La contraseña debe tener al menos 8 caracteres.");
        }

        // Buscar usuario por token
        var usuario = await _usuarioRepository.ObtenerPorTokenRestablecimiento(token)
            ?? throw new ValidacionException("? El link de restablecimiento es inválido o ha expirado.");

        // Verificar que el token no haya expirado
        if (usuario.FechaExpiracionTokenRestablecimiento < DateTime.UtcNow)
        {
            _logger.LogWarning("Intento de restablecer contraseña con token expirado: {Email}", usuario.Email);
            throw new ValidacionException("? El link ha expirado. Por favor, solicita uno nuevo.");
        }

        // Actualizar contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña, workFactor: 12);
        usuario.TokenRestablecimiento = null;
        usuario.FechaExpiracionTokenRestablecimiento = null;

        await _usuarioRepository.Actualizar(usuario);

        _logger.LogInformation("? Contraseña restablecida exitosamente para: {Email}", usuario.Email);

        // Generar JWT
        var token_jwt = GenerarToken(usuario, usuario.Rol.Nombre);

        return new AuthResponseDto
        {
            Token = token_jwt,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }

    // ??????????????????????????????????????????????
    // Generación de JWT
    // ??????????????????????????????????????????????
    private string GenerarToken(Usuario usuario, string nombreRol)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Role, nombreRol),
            new("rolId", usuario.RolId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.ContadorId.HasValue)
            claimsList.Add(new Claim("contadorId", usuario.ContadorId.Value.ToString()));

        var claims = claimsList.ToArray();

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.DuracionMinutos),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponseDto> LoginConGoogle(string? idToken, string? accessToken)
    {
        _logger.LogInformation("LoginConGoogle iniciado | idToken presente: {IdToken} | accessToken presente: {AccessToken}",
            !string.IsNullOrEmpty(idToken), !string.IsNullOrEmpty(accessToken));

        string email;
        string nombre;
        string sub;

        if (!string.IsNullOrEmpty(idToken))
        {
            _logger.LogInformation("LoginConGoogle: validando via id_token");
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _jwtOptions.GoogleClientId }
                    });

                email = payload.Email;
                nombre = payload.Name ?? payload.Email;
                sub = payload.Subject;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "LoginConGoogle: token de Google invalido");
                throw new AccesoNoAutorizadoException("El token de Google no es válido.");
            }
        }
        else if (!string.IsNullOrEmpty(accessToken))
        {
            _logger.LogInformation("LoginConGoogle: validando via access_token (userinfo)");
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var resp = await http.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("LoginConGoogle: userinfo retorno {StatusCode}", (int)resp.StatusCode);
                throw new AccesoNoAutorizadoException("El token de Google no es válido.");
            }

            var json = System.Text.Json.JsonDocument.Parse(body).RootElement;
            email = json.GetProperty("email").GetString()!;
            nombre = json.TryGetProperty("name", out var n) ? n.GetString() ?? email : email;
            sub = json.GetProperty("sub").GetString()!;
        }
        else
        {
            _logger.LogWarning("LoginConGoogle: no se recibio ningun token");
            throw new AccesoNoAutorizadoException("No se recibió ningún token de Google.");
        }

        // Buscar o crear usuario (usa las variables email/nombre, no payload)
        var usuario = await _usuarioRepository.ObtenerPorEmail(email);

        if (usuario is null)
        {
            var rolContador = await _rolRepository.ObtenerPorId(SeedData.RolContadorId)
                ?? throw new EntidadNoEncontradaException("Rol", SeedData.RolContadorId);

            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = email,
                NombreCompleto = nombre,
                PasswordHash = null,
                ProveedorAuth = "Google",
                Estado = "Activo",
                RolId = rolContador.Id,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmado = true // Google ya valida el email
            };

            await _usuarioRepository.Guardar(usuario);
            _logger.LogInformation("LoginConGoogle: nuevo usuario creado via Google | Email: {Email}", email);

            usuario.Rol = rolContador;
        }

        if (usuario.Estado != "Activo")
        {
            _logger.LogWarning("LoginConGoogle: cuenta inactiva para {Email}", email);
            throw new AccesoNoAutorizadoException("La cuenta se encuentra inactiva.");
        }

        // Generar y enviar código 2FA también para Google
        var codigo = GenerarCodigo2FA();
        usuario.Codigo2FA = codigo;
        usuario.FechaExpiracion2FA = DateTime.UtcNow.AddMinutes(5);
        await _usuarioRepository.Actualizar(usuario);

        try
        {
            await _emailService.Enviar2FaCodeAsync(usuario.Email, codigo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar código 2FA (Google) a: {Email}", usuario.Email);
        }

        var tempToken = GenerarTempToken2FA(usuario);
        _logger.LogInformation("LoginConGoogle: 2FA iniciado para {Email}", email);

        return new AuthResponseDto
        {
            Token = null,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = string.Empty,
            Requires2FA = true,
            TempToken = tempToken
        };
    }

    // ??? Verificar código 2FA ?????????????????????????????????????????????????
    public async Task<AuthResponseDto> Verificar2FAAsync(Verificar2FADto dto, string? tokenDispositivoActual)
    {
        // Validar temp token
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(dto.TempToken))
            throw new ValidacionException("Token temporal inválido.");

        JwtSecurityToken jwt;
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            handler.ValidateToken(dto.TempToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);
            jwt = (JwtSecurityToken)validatedToken;
        }
        catch
        {
            throw new ValidacionException("El token temporal ha expirado. Por favor, iniciá sesión nuevamente.");
        }

        var pending2faClaim = jwt.Claims.FirstOrDefault(c => c.Type == "2fa_pending")?.Value;
        if (pending2faClaim != "true")
            throw new ValidacionException("Token temporal inválido.");

        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subClaim, out var usuarioId))
            throw new ValidacionException("Token temporal inválido.");

        var usuario = await _usuarioRepository.ObtenerPorId(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        // Validar código
        if (usuario.Codigo2FA != dto.Codigo)
        {
            _logger.LogWarning("Código 2FA incorrecto para: {Email}", usuario.Email);
            throw new ValidacionException("El código ingresado no es válido.");
        }

        if (usuario.FechaExpiracion2FA < DateTime.UtcNow)
        {
            _logger.LogWarning("Código 2FA expirado para: {Email}", usuario.Email);
            throw new ValidacionException("El código ha expirado. Iniciá sesión nuevamente.");
        }

        // Limpiar código
        usuario.Codigo2FA = null;
        usuario.FechaExpiracion2FA = null;
        await _usuarioRepository.Actualizar(usuario);

        // Limpiar dispositivos expirados del usuario
        await _dispositivoConfiableRepository.EliminarExpiradosPorUsuario(usuario.Id);

        // Si quiere recordar dispositivo, crear token de dispositivo confiable
        string? nuevoTokenDispositivo = null;
        if (dto.RecordarDispositivo)
        {
            nuevoTokenDispositivo = await GenerarTokenDispositivoConfiableAsync(usuario.Id);
        }

        var tokenJwt = GenerarToken(usuario, usuario.Rol.Nombre);
        _logger.LogInformation("2FA verificado exitosamente: {Email}", usuario.Email);

        return new AuthResponseDto
        {
            Token = tokenJwt,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre,
            TempToken = nuevoTokenDispositivo
        };
    }

    public async Task<string> GenerarTokenDispositivoConfiableAsync(Guid usuarioId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var dispositivo = new DispositivoConfiable
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Token = token,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            CreadoEn = DateTime.UtcNow
        };
        await _dispositivoConfiableRepository.Guardar(dispositivo);
        return token;
    }

    // ??? Helpers privados ????????????????????????????????????????????????????
    private static string GenerarCodigo2FA()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    private string GenerarTempToken2FA(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("2fa_pending", "true"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
