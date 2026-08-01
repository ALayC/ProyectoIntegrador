using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class BalanceGeneralPdfService : IBalanceGeneralPdfService
{
    private static readonly string ColorEncabezado = "#34495E";
    private static readonly string ColorActivos = "#27AE60";
    private static readonly string ColorPasivos = "#F39C12";
    private static readonly string ColorPatrimonio = "#3498DB";
    private static readonly string ColorHeaderTabla = "#2C3E50";
    private static readonly string ColorNodoPadre = "#EBF5FF";

    public byte[] Generar(BalanceGeneralViewModel vm)
    {
        var resultado = vm.Resultado!;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().BorderBottom(1).BorderColor(ColorEncabezado).PaddingBottom(4).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Balance General").Bold().FontSize(12).FontColor(ColorEncabezado);
                        if (!string.IsNullOrWhiteSpace(vm.ClienteNombre))
                            col.Item().Text(vm.ClienteNombre).FontSize(8).FontColor("#7F8C8D");
                    });

                    row.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Today:dd/MM/yyyy}").FontSize(7).FontColor("#7F8C8D");
                        if (vm.FechaHasta.HasValue)
                            col.Item().Text($"Fecha hasta: {vm.FechaHasta.Value:dd/MM/yyyy}")
                                .FontSize(7).FontColor("#7F8C8D");
                    });
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Item().PaddingBottom(12).Row(row =>
                    {
                        AgregarTarjeta(row, "Total Activo", resultado.TotalActivo, ColorActivos);
                        row.ConstantItem(8);
                        AgregarTarjeta(row, "Total Pasivo", resultado.TotalPasivo, ColorPasivos);
                        row.ConstantItem(8);
                        AgregarTarjeta(row, "Total Patrimonio", resultado.TotalPatrimonio, ColorPatrimonio);
                    });

                    RenderSeccion(col, "ACTIVOS", ColorActivos, resultado.Activos);
                    col.Item().Height(10);
                    RenderSeccion(col, "PASIVOS", ColorPasivos, resultado.Pasivos);
                    col.Item().Height(10);
                    RenderSeccion(col, "PATRIMONIO", ColorPatrimonio, resultado.Patrimonio);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Sistema Integrado de Gestión Contable · ").FontSize(7).FontColor("#BDC3C7");
                    text.CurrentPageNumber().FontSize(7).FontColor("#BDC3C7");
                    text.Span(" / ").FontSize(7).FontColor("#BDC3C7");
                    text.TotalPages().FontSize(7).FontColor("#BDC3C7");
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void AgregarTarjeta(RowDescriptor row, string label, decimal valor, string color)
    {
        row.RelativeItem().Border(1).BorderColor(color).Column(col =>
        {
            col.Item().Background(color)
                .PaddingVertical(4).PaddingHorizontal(6)
                .AlignCenter()
                .Text(label).Bold().FontSize(7).FontColor(Colors.White);

            col.Item().PaddingVertical(5).PaddingHorizontal(6)
                .AlignCenter()
                .Text(valor.ToString("N2")).Bold().FontSize(10);
        });
    }

    private static void RenderSeccion(
        ColumnDescriptor col,
        string titulo,
        string color,
        List<BalanceGeneralNodoViewModel> nodos)
    {
        col.Item().PaddingBottom(4)
            .Background(color)
            .PaddingVertical(4).PaddingHorizontal(6)
            .Text(titulo).Bold().FontSize(9).FontColor(Colors.White);

        col.Item().PaddingBottom(2).Row(header =>
            RenderHeaderTabla(header));

        foreach (var nodo in nodos)
            RenderNodo(col, nodo, 0);
    }

    private static void RenderHeaderTabla(RowDescriptor row)
    {
        row.RelativeItem(6).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4)
            .Text("Cuenta").Bold().FontSize(7).FontColor(Colors.White);

        row.RelativeItem(2).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4)
            .AlignCenter()
            .Text("Código").Bold().FontSize(7).FontColor(Colors.White);

        row.RelativeItem(2).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4)
            .AlignRight()
            .Text("Saldo").Bold().FontSize(7).FontColor(Colors.White);
    }

    private static void RenderNodo(ColumnDescriptor col, BalanceGeneralNodoViewModel nodo, int nivel)
    {
        var tieneHijas = nodo.Hijas.Count > 0;
        var padding = nivel * 10;
        var bg = tieneHijas ? ColorNodoPadre : "#FFFFFF";

        col.Item().BorderBottom(1).BorderColor("#E8E8E8").Background(bg).Row(row =>
        {
            var celdaNombre = row.RelativeItem(6)
                .PaddingLeft(4 + padding)
                .PaddingVertical(3);

            if (tieneHijas)
                celdaNombre.Text(nodo.Nombre).Bold().FontSize(8);
            else
                celdaNombre.Text(nodo.Nombre).FontSize(8);

            row.RelativeItem(2)
                .PaddingVertical(3).PaddingHorizontal(4)
                .AlignCenter()
                .Text(nodo.Codigo).FontSize(7).FontColor("#7F8C8D");

            var celdaSaldo = row.RelativeItem(2)
                .PaddingVertical(3).PaddingHorizontal(4)
                .AlignRight();

            if (tieneHijas)
                celdaSaldo.Text(nodo.Saldo.ToString("N2")).Bold().FontSize(8);
            else
                celdaSaldo.Text(nodo.Saldo.ToString("N2")).FontSize(8);
        });

        foreach (var hija in nodo.Hijas)
            RenderNodo(col, hija, nivel + 1);
    }
}
