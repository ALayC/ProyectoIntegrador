namespace ProyectoIntegrador.UI.Models;

// ── Vista previa de una línea de asiento parseada del Excel ────────────────
public class LineaImportacionViewModel
{
    public string CodigoCuenta { get; set; } = string.Empty;
    public string NombreCuenta { get; set; } = string.Empty;
    public Guid? CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

// ── Vista previa de un asiento parseado del Excel ──────────────────────────
public class AsientoImportacionViewModel
{
    public int NumAsiento { get; set; }
    public DateOnly Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public List<LineaImportacionViewModel> Lineas { get; set; } = new();

    // Errores de validación de este asiento (null/vacío = válido)
    public List<string> Errores { get; set; } = new();

    public bool EsValido => Errores.Count == 0;

    public decimal TotalDebe => Lineas.Sum(l => l.Debe);
    public decimal TotalHaber => Lineas.Sum(l => l.Haber);
}

// ── Paso 1: Formulario de carga ────────────────────────────────────────────
public class ImportacionIniciarViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid EjercicioId { get; set; }
    public List<EjercicioContableViewModel> Ejercicios { get; set; } = new();
}

// ── Paso 2: Vista previa ───────────────────────────────────────────────────
public class ImportacionVistaPreviaViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public Guid EjercicioId { get; set; }
    public string EjercicioDescripcion { get; set; } = string.Empty;
    public List<AsientoImportacionViewModel> Asientos { get; set; } = new();
    public int TotalValidos => Asientos.Count(a => a.EsValido);
    public int TotalInvalidos => Asientos.Count(a => !a.EsValido);
}

// ── Paso 3: Resultado final ────────────────────────────────────────────────
public class ResultadoImportacionViewModel
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int TotalEnviados { get; set; }
    public int TotalCreados { get; set; }
    public int TotalRechazados { get; set; }
    public int TotalErrores { get; set; }
    public List<ResultadoAsientoViewModel> Resultados { get; set; } = new();
}

public class ResultadoAsientoViewModel
{
    public int NumAsiento { get; set; }
    public string Estado { get; set; } = string.Empty; // "Creado", "Rechazado", "Error"
    public int? NumeroAsientoGenerado { get; set; }
    public string? MensajeError { get; set; }
}

// ── DTO serializado en Session ─────────────────────────────────────────────
public class AsientoParseadoSession
{
    public int NumAsiento { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string Glosa { get; set; } = string.Empty;
    public List<LineaParseadaSession> Lineas { get; set; } = new();
    public bool EsValido { get; set; }
}

public class LineaParseadaSession
{
    public Guid CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}
