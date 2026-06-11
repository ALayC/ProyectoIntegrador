using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class EstadoResultadosPdfService : IEstadoResultadosPdfService
{
    private static readonly string ColorEncabezado = "#34495E";
    private static readonly string ColorIngresos = "#27AE60";
    private static readonly string ColorEgresos = "#E74C3C";
    private static readonly string ColorHeaderTabla = "#2C3E50";
    private static readonly string ColorNodoPadre = "#EBF5FF";

    public byte[] Generar(EstadoResultadosViewModel vm)
    {
        var resultado = vm.Resultado!;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                // ── Header ───────────────────────────────────────
                page.Header().BorderBottom(1).BorderColor(ColorEncabezado).PaddingBottom(4).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Estado de Resultados").Bold().FontSize(12).FontColor(ColorEncabezado);
                        if (!string.IsNullOrWhiteSpace(vm.ClienteNombre))
                            col.Item().Text(vm.ClienteNombre).FontSize(8).FontColor("#7F8C8D");
                    });

                    row.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Today:dd/MM/yyyy}").FontSize(7).FontColor("#7F8C8D");
                        if (vm.FechaDesde.HasValue || vm.FechaHasta.HasValue)
                            col.Item().Text($"Período: {vm.FechaDesde?.ToString("dd/MM/yyyy") ?? "–"} al {vm.FechaHasta?.ToString("dd/MM/yyyy") ?? "–"}")
                                .FontSize(7).FontColor("#7F8C8D");
                    });
                });

                // ── Contenido ─────────────────────────────────────
                page.Content().PaddingTop(10).Column(col =>
                {
                    // Tarjetas resumen
                    col.Item().PaddingBottom(12).Row(row =>
                    {
                        AgregarTarjeta(row, "Total Ingresos", resultado.TotalIngresos, ColorIngresos);
                        row.ConstantItem(8);
                        AgregarTarjeta(row, "Total Egresos", resultado.TotalEgresos, ColorEgresos);
                        row.ConstantItem(8);
                        var colorNeto = resultado.ResultadoNeto >= 0 ? ColorIngresos : ColorEgresos;
                        AgregarTarjeta(row, "Resultado Neto", resultado.ResultadoNeto, colorNeto);
                    });

                    // Sección Ingresos
                    col.Item().PaddingBottom(4)
                        .Background(ColorIngresos)
                        .PaddingVertical(4).PaddingHorizontal(6)
                        .Text("INGRESOS").Bold().FontSize(9).FontColor(Colors.White);

                    col.Item().PaddingBottom(2).Row(header =>
                        RenderHeaderTabla(header));

                    foreach (var nodo in resultado.Ingresos)
                        RenderNodo(col, nodo, 0);

                    col.Item().Height(12);

                    // Sección Egresos
                    col.Item().PaddingBottom(4)
                        .Background(ColorEgresos)
                        .PaddingVertical(4).PaddingHorizontal(6)
                        .Text("EGRESOS").Bold().FontSize(9).FontColor(Colors.White);

                    col.Item().PaddingBottom(2).Row(header =>
                        RenderHeaderTabla(header));

                    foreach (var nodo in resultado.Egresos)
                        RenderNodo(col, nodo, 0);
                });

                // ── Footer ───────────────────────────────────────
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
            .Text("Importe").Bold().FontSize(7).FontColor(Colors.White);
    }

    private static void RenderNodo(ColumnDescriptor col, EstadoResultadoNodoViewModel nodo, int nivel)
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

            var celdaImporte = row.RelativeItem(2)
                .PaddingVertical(3).PaddingHorizontal(4)
                .AlignRight();

            if (tieneHijas)
                celdaImporte.Text(nodo.Importe.ToString("N2")).Bold().FontSize(8);
            else
                celdaImporte.Text(nodo.Importe.ToString("N2")).FontSize(8);
        });

        foreach (var hija in nodo.Hijas)
            RenderNodo(col, hija, nivel + 1);
    }
}