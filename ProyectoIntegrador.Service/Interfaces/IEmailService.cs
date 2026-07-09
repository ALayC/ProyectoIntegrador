namespace ProyectoIntegrador.Service.Interfaces;

/// <summary>
/// Interfaz para servicio de envío de emails.
/// Centraliza la lógica de envío para confirmación, reset y 2FA.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía email de confirmación de cuenta con token.
    /// </summary>
    /// <param name="email">Email del destinatario</param>
    /// <param name="token">Token de confirmación</param>
    /// <param name="baseUrl">URL base de la aplicación (ej: https://app.com)</param>
    Task EnviarConfirmacionEmailAsync(string email, string token, string baseUrl);

    /// <summary>
    /// Envía email de restablecimiento de contraseña con token.
    /// </summary>
    /// <param name="email">Email del destinatario</param>
    /// <param name="token">Token de restablecimiento</param>
    /// <param name="baseUrl">URL base de la aplicación</param>
    Task EnviarRestablecimientoContraseñaAsync(string email, string token, string baseUrl);

    /// <summary>
    /// Envía código 2FA (por email o SMS, según configuración).
    /// </summary>
    /// <param name="email">Email del destinatario</param>
    /// <param name="code">Código 2FA de 6 dígitos</param>
    Task Enviar2FaCodeAsync(string email, string code);

    /// <summary>
    /// Env?a email de invitaci?n para que un usuario se registre como auxiliar contable.
    /// </summary>
    Task EnviarInvitacionAuxiliarAsync(string email, string nombreContador, string baseUrl);
}
