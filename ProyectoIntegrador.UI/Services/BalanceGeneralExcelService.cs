using ClosedXML.Excel;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class BalanceGeneralExcelService : IBalanceGeneralExcelService
{
    public byte[] Generar(BalanceGeneralViewModel vm)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Balance General");

        hoja.Cell(1, 1).Value = "Balance General";
        hoja.Range(1, 1, 1, 3).Merge();
        hoja.Cell(1, 1).Style
            .Font.SetBold(true)
            .Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        hoja.Cell(2, 1).Value = vm.ClienteNombre;
        hoja.Range(2, 1, 2, 3).Merge();
        hoja.Cell(2, 1).Style
            .Font.SetFontColor(XLColor.Gray)
            .Font.SetItalic(true);

        if (vm.FechaHasta.HasValue)
        {
            hoja.Cell(3, 1).Value = $"Fecha hasta: {vm.FechaHasta.Value:dd/MM/yyyy}";
            hoja.Range(3, 1, 3, 3).Merge();
            hoja.Cell(3, 1).Style.Font.SetFontColor(XLColor.Gray).Font.SetItalic(true);
        }

        var resultado = vm.Resultado!;
        var resumenDatos = new[]
        {
            ("Total Activo", resultado.TotalActivo, XLColor.SeaGreen),
            ("Total Pasivo", resultado.TotalPasivo, XLColor.Goldenrod),
            ("Total Patrimonio", resultado.TotalPatrimonio, XLColor.SteelBlue)
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

            valorCell.Value = valor;
            valorCell.Style
                .NumberFormat.SetFormat("#,##0.00")
                .Font.SetBold(true)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        int fila = 8;

        fila = EscribirSeccion(hoja, "ACTIVOS", XLColor.FromArgb(39, 174, 96), resultado.Activos, fila);
        fila++;
        fila = EscribirSeccion(hoja, "PASIVOS", XLColor.FromArgb(243, 156, 18), resultado.Pasivos, fila);
        fila++;
        EscribirSeccion(hoja, "PATRIMONIO", XLColor.FromArgb(52, 152, 219), resultado.Patrimonio, fila);

        hoja.Column(1).Width = 50;
        hoja.Column(2).Width = 20;
        hoja.Column(3).Width = 20;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static int EscribirSeccion(
        IXLWorksheet hoja,
        string titulo,
        XLColor color,
        List<BalanceGeneralNodoViewModel> nodos,
        int fila)
    {
        hoja.Cell(fila, 1).Value = titulo;
        hoja.Range(fila, 1, fila, 3).Merge();
        hoja.Cell(fila, 1).Style
            .Font.SetBold(true)
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(color)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        fila++;

        EscribirEncabezadoColumnas(hoja, ref fila);

        foreach (var nodo in nodos)
            EscribirNodo(hoja, nodo, ref fila, 0);

        return fila;
    }

    private static void EscribirEncabezadoColumnas(IXLWorksheet hoja, ref int fila)
    {
        var headers = new[] { "Cuenta", "Código", "Saldo" };
        for (int c = 0; c < headers.Length; c++)
        {
            hoja.Cell(fila, c + 1).Value = headers[c];
            hoja.Cell(fila, c + 1).Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromArgb(52, 73, 94))
                .Alignment.SetHorizontal(c == 2
                    ? XLAlignmentHorizontalValues.Right
                    : XLAlignmentHorizontalValues.Left);
        }
        fila++;
    }

    private static void EscribirNodo(IXLWorksheet hoja, BalanceGeneralNodoViewModel nodo, ref int fila, int nivel)
    {
        var tieneHijas = nodo.Hijas.Count > 0;
        var indent = new string(' ', nivel * 4);
        var bg = tieneHijas
            ? XLColor.FromArgb(235, 245, 255)
            : (fila % 2 == 0 ? XLColor.FromArgb(245, 245, 245) : XLColor.White);

        var celdaNombre = hoja.Cell(fila, 1);
        celdaNombre.Value = indent + nodo.Nombre;
        celdaNombre.Style
            .Fill.SetBackgroundColor(bg)
            .Font.SetBold(tieneHijas);

        var celdaCodigo = hoja.Cell(fila, 2);
        celdaCodigo.Value = nodo.Codigo;
        celdaCodigo.Style
            .Fill.SetBackgroundColor(bg)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        var celdaSaldo = hoja.Cell(fila, 3);
        celdaSaldo.Value = nodo.Saldo;
        celdaSaldo.Style
            .NumberFormat.SetFormat("#,##0.00")
            .Fill.SetBackgroundColor(bg)
            .Font.SetBold(tieneHijas)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        fila++;

        foreach (var hija in nodo.Hijas)
            EscribirNodo(hoja, hija, ref fila, nivel + 1);
    }
}
