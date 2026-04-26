using Azure.Core;
using Google.Apis.Auth;
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

    public AuthService(
       IUsuarioRepository usuarioRepository,
           IRolRepository rolRepository,
           ITokenRevocadoRepository tokenRevocadoRepository,
       IOptions<JwtOptions> jwtOptions)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _tokenRevocadoRepository = tokenRevocadoRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> Registrar(RegistroDto registroDto)
    {
        // Validar email único
        var existeEmail = await _usuarioRepository.ExisteEmail(registroDto.Email);
        if (existeEmail)
        {
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
        var usuario = await _usuarioRepository.ObtenerPorEmail(loginDto.Email)
 ?? throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");

        // Verificar que tenga password (podría ser usuario Google)
        if (string.IsNullOrEmpty(usuario.PasswordHash))
        {
            throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");
        }

        // Verificar password con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.PasswordHash))
        {
            throw new EntidadNoEncontradaException("Las credenciales proporcionadas no son válidas.");
        }

        // Verificar que el usuario esté activo
        if (usuario.Estado != "Activo")
        {
            throw new AccesoNoAutorizadoException("La cuenta de usuario se encuentra inactiva.");
        }

        // Generar JWT
        var token = GenerarToken(usuario, usuario.Rol.Nombre);

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
        Console.WriteLine($"[Service] idToken? {!string.IsNullOrEmpty(idToken)} | accessToken? {!string.IsNullOrEmpty(accessToken)}");
        Console.WriteLine($"[Service] GoogleClientId config: {_jwtOptions.GoogleClientId}");

        string email;
        string nombre;
        string sub;

        if (!string.IsNullOrEmpty(idToken))
        {
            Console.WriteLine("[Service] Rama: id_token");
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
                Console.WriteLine($"[Service] InvalidJwtException: {ex.Message}");
                throw new AccesoNoAutorizadoException("El token de Google no es válido.");
            }
        }
        else if (!string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("[Service] Rama: access_token → userinfo");
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var resp = await http.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            var body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[Service] userinfo status: {(int)resp.StatusCode} | body: {body}");

            if (!resp.IsSuccessStatusCode)
                throw new AccesoNoAutorizadoException("El token de Google no es válido.");

            var json = System.Text.Json.JsonDocument.Parse(body).RootElement;
            email = json.GetProperty("email").GetString()!;
            nombre = json.TryGetProperty("name", out var n) ? n.GetString() ?? email : email;
            sub = json.GetProperty("sub").GetString()!;
        }
        else
        {
            Console.WriteLine("[Service] Rama: ningún token");
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
            Console.WriteLine("[Service] ✅ Usuario guardado OK");

            usuario.Rol = rolContador;
            Console.WriteLine($"[Service] ✅ Rol asignado: {rolContador?.Nombre ?? "NULL"}");
        }

        if (usuario.Estado != "Activo")
            throw new AccesoNoAutorizadoException("La cuenta se encuentra inactiva.");

        Console.WriteLine($"[Service] ✅ Generando token para rol: {usuario.Rol?.Nombre ?? "NULL"}");
        var token = GenerarToken(usuario, usuario.Rol.Nombre);
        Console.WriteLine("[Service] ✅ Token generado OK, devolviendo respuesta");

        return new AuthResponseDto
        {
            Token = token,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }
}
