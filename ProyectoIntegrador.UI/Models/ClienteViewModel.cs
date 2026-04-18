using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class ClienteViewModel
{
    [Required(ErrorMessage = "El RUT es obligatorio.")]
    [Display(Name = "RUT")]
    public string Rut { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [Display(Name = "Razón social")]
    public string RazonSocial { get; set; } = string.Empty;

    [Display(Name = "Nombre fantasía")]
    public string? NombreFantasia { get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de contribuyente es obligatorio.")]
    [Display(Name = "Tipo de contribuyente")]
    public string TipoContribuyente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La moneda base es obligatoria.")]
    [Display(Name = "Moneda base")]
    public string MonedaBase { get; set; } = string.Empty;
}
