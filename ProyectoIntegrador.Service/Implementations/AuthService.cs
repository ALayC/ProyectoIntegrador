using Azure.Core;
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
using System.Text;

namespace ProyectoIntegrador.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly ITokenRevocadoRepository _tokenRevocadoRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
       IUsuarioRepository usuarioRepository,
           IRolRepository rolRepository,
           ITokenRevocadoRepository tokenRevocadoRepository,
       IOptions<JwtOptions> jwtOptions,
       ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _tokenRevocadoRepository = tokenRevocadoRepository;
        _jwtOptions = jwtOptions.Value;
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

        // Obtener rol Contador por defecto
        var rolContador = await _rolRepository.ObtenerPorId(SeedData.RolContadorId)
?? throw new EntidadNoEncontradaException("Rol", SeedData.RolContadorId);

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
            ContadorId = null, // Contador no tiene ContadorId
            CreatedAt = DateTime.UtcNow
        };

        await _usuarioRepository.Guardar(usuario);

        // Generar JWT
        var token = GenerarToken(usuario, rolContador.Nombre);

        _logger.LogInformation("Usuario registrado correctamente: {Email} | Rol: {Rol}", usuario.Email, rolContador.Nombre);

        return new AuthResponseDto
        {
            Token = token,
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

        // Generar JWT
        var token = GenerarToken(usuario, usuario.Rol.Nombre);

        _logger.LogInformation("Login exitoso: {Email} | Rol: {Rol}", usuario.Email, usuario.Rol.Nombre);

        return new AuthResponseDto
        {
            Token = token,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
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

    // ??????????????????????????????????????????????
    // Generación de JWT
    // ??????????????????????????????????????????????
    private string GenerarToken(Usuario usuario, string nombreRol)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
      {
new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
        new Claim(ClaimTypes.Role, nombreRol),
            new Claim("rolId", usuario.RolId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
     };

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
                CreatedAt = DateTime.UtcNow
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

        var token = GenerarToken(usuario, usuario.Rol.Nombre);
        _logger.LogInformation("LoginConGoogle exitoso: {Email} | Rol: {Rol}", email, usuario.Rol.Nombre);

        return new AuthResponseDto
        {
            Token = token,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }
}
