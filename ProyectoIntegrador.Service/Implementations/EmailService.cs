using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

/// <summary>
/// Servicio para enviar emails (confirmación, reset de contraseña, 2FA).
/// Utiliza SMTP nativo de .NET sin dependencias externas.
/// Configuración esperada en appsettings.json bajo "Email".
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Envía email de confirmación de cuenta con link que incluye token.
    /// </summary>
    public async Task EnviarConfirmacionEmailAsync(string email, string token, string baseUrl)
    {
        try
        {
            var linkConfirmacion = $"{baseUrl}/Auth/ConfirmEmail?token={Uri.EscapeDataString(token)}";

            var cuerpoHtml = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <title>Confirmar Email</title>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
                    <h2 style='color: #333;'>Bienvenido al Sistema Contable</h2>
                    <p style='color: #666; line-height: 1.6;'>
                        Haz clic en el siguiente enlace para confirmar tu dirección de email y completar el registro:
                    </p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{linkConfirmacion}' style='display: inline-block; background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Confirmar Email
                        </a>
                    </p>
                    <p style='color: #999; font-size: 12px;'>
                        O copia y pega este enlace en tu navegador: {linkConfirmacion}
                    </p>
                    <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                        Este enlace expira en 24 horas.
                    </p>
                </div>
            </body>
            </html>";

            await EnviarEmailAsync(
                email,
                "Confirma tu email - Sistema Contable",
                cuerpoHtml,
                isHtml: true);

            _logger.LogInformation("Email de confirmación enviado a: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de confirmación a: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Envía email de restablecimiento de contraseña con link de reset.
    /// </summary>
    public async Task EnviarRestablecimientoContraseñaAsync(string email, string token, string baseUrl)
    {
        try
        {
            var linkReset = $"{baseUrl}/Auth/ResetPassword?token={Uri.EscapeDataString(token)}";

            var cuerpoHtml = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <title>Restablecer Contraseña</title>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
                    <h2 style='color: #333;'>Restablecer Contraseña</h2>
                    <p style='color: #666; line-height: 1.6;'>
                        Recibimos una solicitud para restablecer tu contraseña. Haz clic en el siguiente enlace para crear una nueva:
                    </p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{linkReset}' style='display: inline-block; background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Restablecer Contraseña
                        </a>
                    </p>
                    <p style='color: #999; font-size: 12px;'>
                        O copia y pega este enlace: {linkReset}
                    </p>
                    <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                        Este enlace expira en 1 hora. Si no solicitaste esto, ignora este mensaje.
                    </p>
                </div>
            </body>
            </html>";

            await EnviarEmailAsync(
                email,
                "Restablecer tu contraseña - Sistema Contable",
                cuerpoHtml,
                isHtml: true);

            _logger.LogInformation("Email de restablecimiento de contraseña enviado a: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de reset a: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Envía código 2FA (para SMS o email, según implementación futura).
    /// Por ahora es placeholder para uso futuro con SMS.
    /// </summary>
    public async Task Enviar2FaCodeAsync(string email, string code)
    {
        try
        {
            var cuerpoHtml = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <title>Código 2FA</title>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
                    <h2 style='color: #333;'>Código de Autenticación</h2>
                    <p style='color: #666; line-height: 1.6;'>
                        Tu código de autenticación de dos factores es:
                    </p>
                    <p style='text-align: center; margin: 30px 0; font-size: 28px; font-weight: bold; color: #007bff; letter-spacing: 5px;'>
                        {code}
                    </p>
                    <p style='color: #999; font-size: 12px;'>
                        Este código es válido por 5 minutos.
                    </p>
                </div>
            </body>
            </html>";

            await EnviarEmailAsync(
                email,
                "Tu código 2FA - Sistema Contable",
                cuerpoHtml,
                isHtml: true);

            _logger.LogInformation("Email 2FA enviado a: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar código 2FA a: {Email}", email);
            throw;
        }
    }


    public async Task EnviarInvitacionAuxiliarAsync(string email, string nombreContador, string baseUrl)
    {
        try
        {
            var linkRegistro = $"{baseUrl}/Auth/Register";

            var cuerpoHtml = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <title>Invitacion - Auxiliar Contable</title>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 5px;'>
                    <h2 style='color: #333;'>Invitacion como Auxiliar Contable</h2>
                    <p style='color: #666;'><strong>{nombreContador}</strong> te ha invitado a unirte al sistema como auxiliar contable.</p>
                    <p style='color: #666;'>Registrate con este email (<strong>{email}</strong>) y quedas vinculado automaticamente.</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{linkRegistro}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Registrarme
                        </a>
                    </p>
                    <p style='color: #999; font-size: 12px;'>Invitacion valida por 7 dias.</p>
                </div>
            </body>
            </html>";

            await EnviarEmailAsync(email, $"Invitacion como Auxiliar Contable - {nombreContador}", cuerpoHtml, isHtml: true);
            _logger.LogInformation("Email de invitacion auxiliar enviado a: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar invitacion auxiliar a: {Email}", email);
            throw;
        }
    }
    /// <summary>
    /// Método privado genérico para enviar emails.
    /// </summary>
    private async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpo, bool isHtml = false)
    {
        try
        {
            // Validar configuración
            if (string.IsNullOrWhiteSpace(_emailOptions.SmtpHost))
            {
                _logger.LogWarning(
                    "?? SMTP no configurado. Email NO fue enviado a: {Email}. " +
                    "?? En local: instala Mailhog (localhost:1025). " +
                    "?? En Azure: configura variables de entorno en App Service.",
                    destinatario);
                return; // En desarrollo, permitir que falle silenciosamente
            }

            using var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
            {
                EnableSsl = _emailOptions.UseSsl,
                Credentials = string.IsNullOrWhiteSpace(_emailOptions.SmtpUsername)
                    ? null
                    : new NetworkCredential(_emailOptions.SmtpUsername, _emailOptions.SmtpPassword),
                Timeout = 10000 // 10 segundos
            };

            using var mensaje = new MailMessage
            {
                From = new MailAddress(_emailOptions.RemitenteMail, _emailOptions.RemitenteName),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = isHtml
            };

            mensaje.To.Add(destinatario);

            await client.SendMailAsync(mensaje);

            _logger.LogInformation(
                "? Email enviado exitosamente a: {Email} | Servidor: {Host}:{Port}",
                destinatario,
                _emailOptions.SmtpHost,
                _emailOptions.SmtpPort);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "? Error SMTP al enviar email a: {Email} | Host: {Host}:{Port} | Mensaje: {Mensaje}",
                destinatario,
                _emailOptions.SmtpHost,
                _emailOptions.SmtpPort,
                ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error inesperado al enviar email a: {Email}", destinatario);
            throw;
        }
    }
}
