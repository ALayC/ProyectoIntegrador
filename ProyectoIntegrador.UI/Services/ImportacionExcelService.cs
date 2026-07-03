using ClosedXML.Excel;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class ImportacionFormatoException : Exception
{
    public ImportacionFormatoException(string mensaje) : base(mensaje) { }
}

public class ImportacionExcelService : IImportacionExcelService
{
    // Columnas esperadas en el Excel (índice base-1)
    private const int ColNumAsiento = 1;
    private const int ColFecha = 2;
    private const int ColGlosa = 3;
    private const int ColCodigo = 4;
    private const int ColNombreCuenta = 5; // auto-completado por fórmula VLOOKUP
    private const int ColDebe = 6;
    private const int ColHaber = 7;
    private const int ColMoneda = 8;
    private const int ColTipoCambio = 9;

    private static readonly string[] EncabezadosEsperados =
        ["NumAsiento", "Fecha", "Glosa", "CodigoCuenta", "NombreCuenta", "Debe", "Haber", "Moneda", "TipoCambio"];

    // ── Generación del template ────────────────────────────────────────────

    public byte[] GenerarTemplate(string clienteNombre, List<CuentaContableViewModel> cuentas)
    {
        using var workbook = new XLWorkbook();

        // Hoja 2 (oculta): catálogo de cuentas para el dropdown
        var hojaCuentas = workbook.Worksheets.Add("Cuentas");
        hojaCuentas.Visibility = XLWorksheetVisibility.VeryHidden;

        hojaCuentas.Cell(1, 1).Value = "Codigo";
        hojaCuentas.Cell(1, 2).Value = "Nombre";
        hojaCuentas.Cell(1, 3).Value = "Tipo";
        hojaCuentas.Cell(1, 1).Style.Font.SetBold(true);
        hojaCuentas.Cell(1, 2).Style.Font.SetBold(true);
        hojaCuentas.Cell(1, 3).Style.Font.SetBold(true);

        for (int i = 0; i < cuentas.Count; i++)
        {
            hojaCuentas.Cell(i + 2, 1).Value = cuentas[i].Codigo;
            hojaCuentas.Cell(i + 2, 2).Value = cuentas[i].Nombre;
            hojaCuentas.Cell(i + 2, 3).Value = cuentas[i].Tipo;
        }

        hojaCuentas.Columns().AdjustToContents();

        // Hoja 1: Importacion (donde el usuario carga datos)
        var hojaImport = workbook.Worksheets.Add("Importacion");
        // Mover Importacion a posición 1 (antes de Cuentas)
        hojaImport.Position = 1;

        // Título
        hojaImport.Cell(1, 1).Value = $"Importación de asientos — {clienteNombre}";
        hojaImport.Range(1, 1, 1, 9).Merge();
        hojaImport.Cell(1, 1).Style
            .Font.SetBold(true)
            .Font.SetFontSize(13)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#206bc4"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        // Instrucciones
        hojaImport.Cell(2, 1).Value = "• NumAsiento: número entero que agrupa las líneas de un mismo asiento.";
        hojaImport.Cell(3, 1).Value = "• CodigoCuenta: seleccione del desplegable. La columna NombreCuenta se completa automáticamente.";
        hojaImport.Cell(4, 1).Value = "• Debe y Haber: valores positivos. Cada asiento debe balancear (Debe = Haber en moneda base). Moneda: UYU/USD. TipoCambio: 1 para UYU.";
        for (int r = 2; r <= 4; r++)
        {
            hojaImport.Range(r, 1, r, 9).Merge();
            hojaImport.Cell(r, 1).Style
                .Font.SetItalic(true)
                .Font.SetFontColor(XLColor.FromHtml("#626976"))
                .Font.SetFontSize(10);
        }

        // Encabezados de datos (fila 6)
        int filaEncabezado = 6;
        string[] encabezados = ["NumAsiento", "Fecha", "Glosa", "CodigoCuenta", "NombreCuenta", "Debe", "Haber", "Moneda", "TipoCambio"];
        for (int c = 0; c < encabezados.Length; c++)
        {
            var cell = hojaImport.Cell(filaEncabezado, c + 1);
            cell.Value = encabezados[c];
            cell.Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#206bc4"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Border.SetBottomBorder(XLBorderStyleValues.Medium);
        }

        // Filas de ejemplo (filas 7-10) + validaciones
        int filaInicio = 7;
        int filaFin = 506; // 500 filas de datos posibles

        // Ejemplos
        var ejemplos = new (int num, string fecha, string glosa, string cod, decimal debe, decimal haber)[]
        {
            (1, DateTime.Today.ToString("dd/MM/yyyy"), "Ejemplo: compra de insumos", cuentas.Count > 0 ? cuentas[0].Codigo : "1.1.01", 10000, 0),
            (1, DateTime.Today.ToString("dd/MM/yyyy"), "Ejemplo: compra de insumos", cuentas.Count > 1 ? cuentas[1].Codigo : "2.1.01", 0, 10000),
        };

        for (int i = 0; i < ejemplos.Length; i++)
        {
            var fila = filaInicio + i;
            var ej = ejemplos[i];
            hojaImport.Cell(fila, ColNumAsiento).Value = ej.num;
            hojaImport.Cell(fila, ColFecha).Value = ej.fecha;
            hojaImport.Cell(fila, ColGlosa).Value = ej.glosa;
            hojaImport.Cell(fila, ColCodigo).Value = ej.cod;
            hojaImport.Cell(fila, ColDebe).Value = ej.debe;
            hojaImport.Cell(fila, ColHaber).Value = ej.haber;
            hojaImport.Cell(fila, ColMoneda).Value = "UYU";
            hojaImport.Cell(fila, ColTipoCambio).Value = 1;

            // Color tenue de ejemplo
            hojaImport.Range(fila, 1, fila, 9).Style
                .Fill.SetBackgroundColor(XLColor.FromHtml("#f0f4ff"))
                .Font.SetFontColor(XLColor.FromHtml("#626976"));
        }

        // Dropdown en columna D para el rango de datos
        if (cuentas.Count > 0)
        {
            var codigosRange = $"Cuentas!$A$2:$A${cuentas.Count + 1}";
            var rangoDropdown = hojaImport.Range(filaInicio, ColCodigo, filaFin, ColCodigo);
            rangoDropdown.SetDataValidation().List($"{codigosRange}", true);
        }

        // Fórmula VLOOKUP en columna NombreCuenta: se auto-completa al elegir el código
        for (int r = filaInicio; r <= filaFin; r++)
        {
            hojaImport.Cell(r, ColNombreCuenta).FormulaA1 =
                $"=IF(D{r}=\"\",\"\",IFERROR(VLOOKUP(D{r},Cuentas!$A:$B,2,0),\"— código no encontrado —\"))";
        }

        // Estilo de la columna NombreCuenta: fondo azul claro, cursiva (solo lectura visual)
        hojaImport.Range(filaInicio, ColNombreCuenta, filaFin, ColNombreCuenta).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#eef2fb"))
            .Font.SetFontColor(XLColor.FromHtml("#3d5a9e"))
            .Font.SetItalic(true);

        // Encabezado NombreCuenta con color diferente para distinguirlo como columna automática
        hojaImport.Cell(filaEncabezado, ColNombreCuenta).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#4a7ed4"));

        // Formato de fecha en columna B
        hojaImport.Range(filaInicio, ColFecha, filaFin, ColFecha)
            .Style.NumberFormat.SetFormat("DD/MM/YYYY");

        // Formato numérico en columnas Debe, Haber y TipoCambio
        hojaImport.Range(filaInicio, ColDebe, filaFin, ColDebe)
            .Style.NumberFormat.SetFormat("#,##0.00");
        hojaImport.Range(filaInicio, ColHaber, filaFin, ColHaber)
            .Style.NumberFormat.SetFormat("#,##0.00");
        hojaImport.Range(filaInicio, ColTipoCambio, filaFin, ColTipoCambio)
            .Style.NumberFormat.SetFormat("#,##0.000000");

        // Dropdown en columna Moneda
        var rangoMoneda = hojaImport.Range(filaInicio, ColMoneda, filaFin, ColMoneda);
        rangoMoneda.SetDataValidation().List("\"UYU,USD\"", true);

        // Anchos de columna
        hojaImport.Column(ColNumAsiento).Width = 14;
        hojaImport.Column(ColFecha).Width = 14;
        hojaImport.Column(ColGlosa).Width = 45;
        hojaImport.Column(ColCodigo).Width = 18;
        hojaImport.Column(ColNombreCuenta).Width = 38;
        hojaImport.Column(ColDebe).Width = 16;
        hojaImport.Column(ColHaber).Width = 16;
        hojaImport.Column(ColMoneda).Width = 10;
        hojaImport.Column(ColTipoCambio).Width = 14;

        // Inmovilizar fila de encabezado
        hojaImport.SheetView.FreezeRows(filaEncabezado);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Parseo del archivo subido ──────────────────────────────────────────

    public List<AsientoImportacionViewModel> Parsear(
        Stream archivoExcel,
        Dictionary<string, CuentaContableViewModel> cuentasPorCodigo)
    {
        using var workbook = new XLWorkbook(archivoExcel);

        var hoja = workbook.Worksheets.FirstOrDefault(w =>
            w.Name.Equals("Importacion", StringComparison.OrdinalIgnoreCase))
            ?? throw new ImportacionFormatoException(
                "El archivo no contiene la hoja 'Importacion'. Asegúrese de usar el template descargado desde el sistema.");

        // Detectar la fila de encabezados buscando "NumAsiento"
        int filaEncabezado = BuscarFilaEncabezado(hoja);

        // Validar que todos los encabezados esperados existan
        ValidarEncabezados(hoja, filaEncabezado);

        // Leer filas de datos
        var filasRaw = LeerFilas(hoja, filaEncabezado + 1);

        if (filasRaw.Count == 0)
            throw new ImportacionFormatoException(
                "El archivo no contiene filas de datos. Complete al menos un asiento antes de subir el archivo.");

        // Agrupar por NumAsiento y construir ViewModels
        return ConstruirAsientos(filasRaw, cuentasPorCodigo);
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    private static int BuscarFilaEncabezado(IXLWorksheet hoja)
    {
        for (int r = 1; r <= 20; r++)
        {
            var valor = hoja.Cell(r, ColNumAsiento).GetString().Trim();
            if (valor.Equals("NumAsiento", StringComparison.OrdinalIgnoreCase))
                return r;
        }

        throw new ImportacionFormatoException(
            "No se encontró la fila de encabezados (columna 'NumAsiento') en las primeras 20 filas. " +
            "Asegúrese de usar el template descargado desde el sistema sin modificar la estructura.");
    }

    private static void ValidarEncabezados(IXLWorksheet hoja, int fila)
    {
        var faltantes = new List<string>();
        for (int c = 0; c < EncabezadosEsperados.Length; c++)
        {
            var valor = hoja.Cell(fila, c + 1).GetString().Trim();
            if (!valor.Equals(EncabezadosEsperados[c], StringComparison.OrdinalIgnoreCase))
                faltantes.Add(EncabezadosEsperados[c]);
        }

        if (faltantes.Count > 0)
            throw new ImportacionFormatoException(
                $"Columnas incorrectas o en orden incorrecto: {string.Join(", ", faltantes)}. " +
                "El orden debe ser: NumAsiento, Fecha, Glosa, CodigoCuenta, NombreCuenta, Debe, Haber.");
    }

    private sealed record FilaRaw(int NumAsiento, DateOnly Fecha, string Glosa, string CodigoCuenta, decimal Debe, decimal Haber, string Moneda, decimal TipoCambio, int NumFila);

    private static List<FilaRaw> LeerFilas(IXLWorksheet hoja, int filaInicio)
    {
        var filas = new List<FilaRaw>();
        int filaActual = filaInicio;

        while (true)
        {
            // Parar en la primera fila completamente vacía
            var celdaNum = hoja.Cell(filaActual, ColNumAsiento);
            if (celdaNum.IsEmpty() || string.IsNullOrWhiteSpace(celdaNum.GetString()))
                break;

            // NumAsiento
            if (!int.TryParse(celdaNum.GetString().Trim(), out int numAsiento) || numAsiento <= 0)
                throw new ImportacionFormatoException(
                    $"Fila {filaActual}: 'NumAsiento' debe ser un número entero positivo. Valor encontrado: '{celdaNum.GetString()}'.");

            // Fecha
            DateOnly fecha;
            var celdaFecha = hoja.Cell(filaActual, ColFecha);
            if (celdaFecha.DataType == XLDataType.DateTime)
            {
                fecha = DateOnly.FromDateTime(celdaFecha.GetDateTime());
            }
            else if (DateOnly.TryParseExact(celdaFecha.GetString().Trim(), ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"], null, System.Globalization.DateTimeStyles.None, out var fechaParsed))
            {
                fecha = fechaParsed;
            }
            else
            {
                throw new ImportacionFormatoException(
                    $"Fila {filaActual}: 'Fecha' no tiene formato válido (dd/MM/yyyy). Valor encontrado: '{celdaFecha.GetString()}'.");
            }

            // Glosa
            var glosa = hoja.Cell(filaActual, ColGlosa).GetString().Trim();
            if (string.IsNullOrWhiteSpace(glosa))
                throw new ImportacionFormatoException(
                    $"Fila {filaActual}: 'Glosa' es obligatoria.");

            // CodigoCuenta
            var codigo = hoja.Cell(filaActual, ColCodigo).GetString().Trim();
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ImportacionFormatoException(
                    $"Fila {filaActual}: 'CodigoCuenta' es obligatorio.");

            // Debe
            var celdaDebe = hoja.Cell(filaActual, ColDebe);
            decimal debe = celdaDebe.DataType == XLDataType.Number
                ? (decimal)celdaDebe.GetDouble()
                : decimal.TryParse(celdaDebe.GetString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0m;

            // Haber
            var celdaHaber = hoja.Cell(filaActual, ColHaber);
            decimal haber = celdaHaber.DataType == XLDataType.Number
                ? (decimal)celdaHaber.GetDouble()
                : decimal.TryParse(celdaHaber.GetString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var h) ? h : 0m;

            // Moneda
            var moneda = hoja.Cell(filaActual, ColMoneda).GetString().Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(moneda))
                moneda = "UYU";

            // TipoCambio
            var celdaTc = hoja.Cell(filaActual, ColTipoCambio);
            decimal tipoCambio = celdaTc.DataType == XLDataType.Number
                ? (decimal)celdaTc.GetDouble()
                : decimal.TryParse(celdaTc.GetString().Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tc) ? tc : 1m;
            if (tipoCambio <= 0)
                tipoCambio = 1m;

            filas.Add(new FilaRaw(numAsiento, fecha, glosa, codigo, debe, haber, moneda, tipoCambio, filaActual));
            filaActual++;
        }

        return filas;
    }

    private static List<AsientoImportacionViewModel> ConstruirAsientos(
        List<FilaRaw> filas,
        Dictionary<string, CuentaContableViewModel> cuentasPorCodigo)
    {
        var grupos = filas.GroupBy(f => f.NumAsiento).OrderBy(g => g.Key);
        var resultado = new List<AsientoImportacionViewModel>();

        foreach (var grupo in grupos)
        {
            var primeraFila = grupo.First();
            var asiento = new AsientoImportacionViewModel
            {
                NumAsiento = grupo.Key,
                Fecha = primeraFila.Fecha,
                Glosa = primeraFila.Glosa,
            };

            var codigosUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fila in grupo)
            {
                CuentaContableViewModel? cuenta = null;
                cuentasPorCodigo.TryGetValue(fila.CodigoCuenta, out cuenta);

                var linea = new LineaImportacionViewModel
                {
                    CodigoCuenta = fila.CodigoCuenta,
                    NombreCuenta = cuenta?.Nombre ?? string.Empty,
                    CuentaContableId = cuenta?.Id,
                    Debe = fila.Debe,
                    Haber = fila.Haber,
                    Moneda = fila.Moneda,
                    TipoCambio = fila.TipoCambio
                };

                asiento.Lineas.Add(linea);

                // Validaciones por línea
                if (cuenta is null)
                    asiento.Errores.Add($"El código '{fila.CodigoCuenta}' no existe en el plan de cuentas del cliente.");
                else if (!codigosUsados.Add(fila.CodigoCuenta))
                    asiento.Errores.Add($"El código '{fila.CodigoCuenta}' aparece más de una vez en el asiento.");

                if (fila.Debe < 0 || fila.Haber < 0)
                    asiento.Errores.Add($"Fila {fila.NumFila}: los importes no pueden ser negativos.");

                if (fila.Debe > 0 && fila.Haber > 0)
                    asiento.Errores.Add($"Fila {fila.NumFila}: una línea no puede tener importe en Debe y Haber simultáneamente.");

                if (fila.Debe == 0 && fila.Haber == 0)
                    asiento.Errores.Add($"Fila {fila.NumFila}: la línea no puede tener Debe y Haber en cero.");
            }

            // Validaciones por asiento
            if (asiento.Lineas.Count < 2)
                asiento.Errores.Add("El asiento debe tener al menos 2 líneas.");

            if (asiento.TotalDebe != asiento.TotalHaber)
                asiento.Errores.Add($"El asiento está desbalanceado: Debe={asiento.TotalDebe:N2}, Haber={asiento.TotalHaber:N2}.");

            // Deduplicar errores
            asiento.Errores = asiento.Errores.Distinct().ToList();

            resultado.Add(asiento);
        }

        return resultado;
    }
}
