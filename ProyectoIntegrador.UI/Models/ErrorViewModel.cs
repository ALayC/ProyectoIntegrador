namespace ProyectoIntegrador.UI.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? MensajeError { get; set; }
    }
}
