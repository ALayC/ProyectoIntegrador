namespace ProyectoIntegrador.Service.DTOs;

/// <summary>Una línea individual dentro del bulk de importación, ya resuelta a IDs.</summary>
public class LineaImportacionDto
{
    public Guid CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

/// <summary>Un asiento completo dentro del bulk de importación.</summary>
public class AsientoImportacionDto
{
    public int NumAsiento { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public List<LineaImportacionDto> Lineas { get; set; } = new();
}

/// <summary>Request para la importación masiva de asientos contables.</summary>
public class ImportarAsientosBulkDto
{
    public Guid ClienteId { get; set; }
    public Guid EjercicioId { get; set; }
    public List<AsientoImportacionDto> Asientos { get; set; } = new();
}

/// <summary>Resultado de un asiento individual dentro del bulk.</summary>
public class ResultadoAsientoImportadoDto
{
    public int NumAsiento { get; set; }
    public bool Exitoso { get; set; }
    public int? NumeroAsientoGenerado { get; set; }
    public Guid? AsientoId { get; set; }
    public string? MensajeError { get; set; }
}

/// <summary>Resultado completo de una importación masiva.</summary>
public class ResultadoImportacionBulkDto
{
    public int TotalEnviados { get; set; }
    public int TotalCreados { get; set; }
    public int TotalErrores { get; set; }
    public List<ResultadoAsientoImportadoDto> Resultados { get; set; } = new();
}
