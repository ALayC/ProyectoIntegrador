using ClosedXML.Excel;
using ProyectoIntegrador.UI.Models;

using System.Drawing;

namespace ProyectoIntegrador.UI.Services;

public class LibroMayorExcelService : ILibroMayorExcelService
{
    public byte[] Generar(LibroMayorViewModel vm)
    {
        using var workbook = new XLWorkbook();

        foreach (var cuenta in vm.CuentasMayor)
        {
            var nombreHoja = $"{cuenta.Codigo} - {cuenta.Nombre}"
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("*", "")
                .Replace("?", "")
                .Replace("[", "")
                .Replace("]", "");

            if (nombreHoja.Length > 31)
                nombreHoja = nombreHoja[..31];

            var hoja = workbook.Worksheets.Add(nombreHoja);

            // ── Título ──────────────────────────────────────────────
            hoja.Cell(1, 1).Value = $"{cuenta.Codigo} – {cuenta.Nombre}";
            hoja.Range(1, 1, 1, 8).Merge();
            hoja.Cell(1, 1).Style
                .Font.SetBold(true)
                .Font.SetFontSize(13)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // ── Subtítulo ────────────────────────────────────────────
            hoja.Cell(2, 1).Value = $"Tipo: {cuenta.Tipo}   |   Naturaleza: {cuenta.Naturaleza}";
            hoja.Range(2, 1, 2, 8).Merge();
            hoja.Cell(2, 1).Style
                .Font.SetFontColor(XLColor.Gray)
                .Font.SetItalic(true);

            // ── Resumen ──────────────────────────────────────────────
            var resumenLabels = new[] { "Saldo inicial", "Débitos", "Créditos" };
            var resumenValores = new[] { cuenta.SaldoInicial, cuenta.Debitos, cuenta.Creditos };

            // etiqueta y valor para saldo final (según Debitos vs Creditos)
            string saldoFinalLabel;
            decimal saldoFinalValor;
            if (cuenta.Debitos > cuenta.Creditos) {
                saldoFinalLabel = "Saldo deudor";
                saldoFinalValor = cuenta.Debitos - cuenta.Creditos;
            } else if (cuenta.Creditos > cuenta.Debitos) {
                saldoFinalLabel = "Saldo acreedor";
                saldoFinalValor = cuenta.Creditos - cuenta.Debitos;
            } else {
                saldoFinalLabel = "Saldo deudor";
                saldoFinalValor = 0m;
            }

            var resumenColores = new[] { XLColor.SteelBlue, XLColor.SeaGreen, XLColor.IndianRed, XLColor.RoyalBlue };

            for (int col = 0; col < 3; col++)
            {
                var labelCell = hoja.Cell(4, col * 2 + 1);
                var valorCell = hoja.Cell(5, col * 2 + 1);

                hoja.Range(4, col * 2 + 1, 4, col * 2 + 2).Merge();
                hoja.Range(5, col * 2 + 1, 5, col * 2 + 2).Merge();

                labelCell.Value = resumenLabels[col];
                labelCell.Style
                    .Font.SetBold(true)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(resumenColores[col])
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                valorCell.Value = resumenValores[col];
                valorCell.Style
                    .NumberFormat.SetFormat("#,##0.00")
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetBold(true);
            }

            // Saldo final (columna 4)
            var labelCellFinal = hoja.Cell(4, 3 * 2 + 1);
            var valorCellFinal = hoja.Cell(5, 3 * 2 + 1);

            hoja.Range(4, 3 * 2 + 1, 4, 3 * 2 + 2).Merge();
            hoja.Range(5, 3 * 2 + 1, 5, 3 * 2 + 2).Merge();

            labelCellFinal.Value = saldoFinalLabel;
            labelCellFinal.Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(resumenColores[3])
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            valorCellFinal.Value = saldoFinalValor;
            valorCellFinal.Style
                .NumberFormat.SetFormat("#,##0.00")
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Font.SetBold(true);

            // ── Header tabla ─────────────────────────────────────────
            var headers = new[] { "Fecha", "Asiento", "Glosa", "Debe", "Haber", "Saldo", "Moneda", "Saldo base" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = hoja.Cell(7, col + 1);
                cell.Value = headers[col];
                cell.Style
                    .Font.SetBold(true)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(52, 73, 94))
                    .Alignment.SetHorizontal(col >= 3
                        ? XLAlignmentHorizontalValues.Right
                        : XLAlignmentHorizontalValues.Left);
            }

            // ── Movimientos ──────────────────────────────────────────
            int fila = 8;
            foreach (var mov in cuenta.Movimientos)
            {
                hoja.Cell(fila, 1).Value = mov.Fecha.ToString("dd/MM/yyyy");
                hoja.Cell(fila, 2).Value = mov.NumeroAsiento;
                hoja.Cell(fila, 3).Value = mov.Glosa;
                hoja.Cell(fila, 4).Value = mov.Debe;
                hoja.Cell(fila, 5).Value = mov.Haber;
                hoja.Cell(fila, 6).Value = mov.SaldoAcumulado;
                hoja.Cell(fila, 7).Value = mov.Moneda;
                hoja.Cell(fila, 8).Value = mov.SaldoAcumuladoBase;

                foreach (int c in new[] { 4, 5, 6, 8 })
                    hoja.Cell(fila, c).Style.NumberFormat.SetFormat("#,##0.00");

                foreach (int c in new[] { 4, 5, 6, 8 })
                    hoja.Cell(fila, c).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                if (fila % 2 == 0)
                    hoja.Range(fila, 1, fila, 8).Style
                        .Fill.SetBackgroundColor(XLColor.FromArgb(245, 245, 245));

                fila++;
            }

            // ── Borde en tabla ───────────────────────────────────────
            if (cuenta.Movimientos.Count > 0)
            {
                hoja.Range(7, 1, fila - 1, 8).Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair);
            }

            // ── Anchos de columna ────────────────────────────────────
            hoja.Column(1).Width = 14;
            hoja.Column(2).Width = 12;
            hoja.Column(3).Width = 40;
            hoja.Column(4).Width = 14;
            hoja.Column(5).Width = 14;
            hoja.Column(6).Width = 14;
            hoja.Column(8).Width = 14;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}