using ClosedXML.Excel;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class ReporteCierreExcelService : IReporteCierreExcelService
{
    public byte[] Generar(ReporteCierreViewModel vm)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Asientos de Cierre");

        // ── Título ───────────────────────────────────────────────
        hoja.Cell(1, 1).Value = "Asientos de Cierre";
        hoja.Range(1, 1, 1, 5).Merge();
        hoja.Cell(1, 1).Style
            .Font.SetBold(true)
            .Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        hoja.Cell(2, 1).Value = vm.ClienteNombre;
        hoja.Range(2, 1, 2, 5).Merge();
        hoja.Cell(2, 1).Style
            .Font.SetFontColor(XLColor.Gray)
            .Font.SetItalic(true);

        hoja.Cell(3, 1).Value = $"Período: {vm.Ejercicio.FechaInicio:dd/MM/yyyy}  –  {vm.Ejercicio.FechaFin:dd/MM/yyyy}";
        hoja.Range(3, 1, 3, 5).Merge();
        hoja.Cell(3, 1).Style.Font.SetFontColor(XLColor.Gray).Font.SetItalic(true);

        // ── Resumen ──────────────────────────────────────────────
        var totalDebe = vm.Asientos.Sum(a => a.TotalDebe);
        var totalHaber = vm.Asientos.Sum(a => a.TotalHaber);
        var resumenDatos = new[]
        {
            ("Asientos Generados", (decimal)vm.Asientos.Count, XLColor.FromArgb(52, 100, 145)),
            ("Total Debe",  totalDebe, XLColor.SeaGreen),
            ("Total Haber", totalHaber, XLColor.IndianRed)
        };

        for (int i = 0; i < resumenDatos.Length; i++)
        {
            var (label, valor, color) = resumenDatos[i];
            var labelCell = hoja.Cell(5, i + 1);
            var valorCell = hoja.Cell(6, i + 1);

            labelCell.Value = label;
            labelCell.Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(color)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            valorCell.Value = i == 0 ? (int)valor : valor;
            if (i > 0)
                valorCell.Style.NumberFormat.SetFormat("#,##0.00");
            valorCell.Style
                .Font.SetBold(true)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        // ── Asientos ─────────────────────────────────────────────
        int fila = 8;

        foreach (var asiento in vm.Asientos)
        {
            // Encabezado del asiento
            hoja.Cell(fila, 1).Value = $"Asiento N° {asiento.Numero}  —  {asiento.Glosa}";
            hoja.Range(fila, 1, fila, 4).Merge();
            hoja.Cell(fila, 5).Value = asiento.Fecha.ToString("dd/MM/yyyy");
            hoja.Row(fila).Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(30, 55, 100));
            hoja.Cell(fila, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            fila++;

            // Cabecera de columnas
            var headers = new[] { "Código", "Cuenta", string.Empty, "Debe", "Haber" };
            for (int c = 0; c < headers.Length; c++)
            {
                hoja.Cell(fila, c + 1).Value = headers[c];
                hoja.Cell(fila, c + 1).Style
                    .Font.SetBold(true)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(52, 73, 94))
                    .Alignment.SetHorizontal(c >= 3 ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Left);
            }
            fila++;

            // Líneas
            foreach (var linea in asiento.Lineas)
            {
                var bg = fila % 2 == 0 ? XLColor.FromArgb(245, 245, 245) : XLColor.White;
                hoja.Cell(fila, 1).Value = linea.CodigoCuenta;
                hoja.Cell(fila, 2).Value = linea.NombreCuenta;
                hoja.Range(fila, 2, fila, 3).Merge();

                var celdaDebe = hoja.Cell(fila, 4);
                var celdaHaber = hoja.Cell(fila, 5);

                if (linea.Debe != 0)
                {
                    celdaDebe.Value = linea.Debe;
                    celdaDebe.Style.NumberFormat.SetFormat("#,##0.00").Font.SetFontColor(XLColor.SeaGreen);
                }
                else
                {
                    celdaDebe.Value = "—";
                }

                if (linea.Haber != 0)
                {
                    celdaHaber.Value = linea.Haber;
                    celdaHaber.Style.NumberFormat.SetFormat("#,##0.00").Font.SetFontColor(XLColor.IndianRed);
                }
                else
                {
                    celdaHaber.Value = "—";
                }

                hoja.Row(fila).Style.Fill.SetBackgroundColor(bg);
                hoja.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                hoja.Cell(fila, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                fila++;
            }

            // Fila totales
            hoja.Cell(fila, 3).Value = "TOTALES";
            hoja.Cell(fila, 4).Value = asiento.TotalDebe;
            hoja.Cell(fila, 5).Value = asiento.TotalHaber;
            hoja.Cell(fila, 3).Style.Font.SetBold(true);
            hoja.Cell(fila, 4).Style.NumberFormat.SetFormat("#,##0.00").Font.SetBold(true).Font.SetFontColor(XLColor.SeaGreen).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            hoja.Cell(fila, 5).Style.NumberFormat.SetFormat("#,##0.00").Font.SetBold(true).Font.SetFontColor(XLColor.IndianRed).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            fila += 2;
        }

        // ── Anchos de columna ─────────────────────────────────────
        hoja.Column(1).Width = 12;
        hoja.Column(2).Width = 35;
        hoja.Column(3).Width = 10;
        hoja.Column(4).Width = 18;
        hoja.Column(5).Width = 18;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
