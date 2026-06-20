using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.UI.Models;

public class CuentaContableViewModel
{
    public Guid Id { get; set; }
    public Guid PlanCuentasId { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La naturaleza es obligatoria.")]
    [Display(Name = "Naturaleza")]
    public string Naturaleza { get; set; } = string.Empty;

    [Display(Name = "Es imputable")]
    public bool EsImputable { get; set; }

    public bool EsSistema { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = string.Empty;

    public Guid? CuentaPadreId { get; set; }
}

public class CuentaContableFormViewModel
{
    public CuentaContableViewModel Cuenta { get; set; } = new();
    public CuentaContableResumenViewModel? CuentaPadre { get; set; }
}

public class CuentaContableResumenViewModel
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class CodigoSugeridoDto
{
    public string Codigo { get; set; } = string.Empty;
}

public class CuentaContableArbolViewModel
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsImputable { get; set; }
    public bool EsSistema { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<CuentaContableArbolViewModel> Hijas { get; set; } = new();
    public Guid PlanCuentasId { get; set; }
    public Guid CuentaPadreId { get; set; }
}