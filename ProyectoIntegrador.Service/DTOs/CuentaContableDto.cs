using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO de respuesta para cuentas contables.
/// </summary>
public class CuentaContableResponseDto
{
    public Guid Id { get; set; }
    public Guid PlanCuentasId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public bool EsImputable { get; set; }
    public bool EsSistema { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? CuentaPadreId { get; set; }
}

/// <summary>
/// DTO de entrada para crear una cuenta contable.
/// </summary>
public class CrearCuentaContableDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La naturaleza es obligatoria.")]
    public string Naturaleza { get; set; } = string.Empty;

    public bool EsImputable { get; set; }

    public string? Estado { get; set; }

    public Guid? CuentaPadreId { get; set; }
}

/// <summary>
/// DTO de entrada para actualizar una cuenta contable.
/// </summary>
public class ActualizarCuentaContableDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La naturaleza es obligatoria.")]
    public string Naturaleza { get; set; } = string.Empty;

    public bool EsImputable { get; set; }

    public string? Estado { get; set; }

    public Guid? CuentaPadreId { get; set; }
}

/// <summary>
/// DTO para representar cuentas contables en estructura de árbol.
/// </summary>
public class CuentaContableArbolDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsSistema { get; set; }
    public bool EsImputable { get; set; }
    public string Estado { get; set; } = string.Empty;

    public List<CuentaContableArbolDto> Hijas { get; set; } = new();
}