using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class ReporteCierrePdfService : IReporteCierrePdfService
{
    private static readonly string ColorEncabezado = "#1E3764";
    private static readonly string ColorHeaderTabla = "#2C3E50";
    private static readonly string ColorDebe = "#27AE60";
    private static readonly string ColorHaber = "#E74C3C";

    public byte[] Generar(ReporteCierreViewModel vm)
    {
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
                        col.Item().Text("Asientos de Cierre").Bold().FontSize(12).FontColor(ColorEncabezado);
                        if (!string.IsNullOrWhiteSpace(vm.ClienteNombre))
                            col.Item().Text(vm.ClienteNombre).FontSize(8).FontColor("#7F8C8D");
                        col.Item().Text($"Período: {vm.Ejercicio.FechaInicio:dd/MM/yyyy} – {vm.Ejercicio.FechaFin:dd/MM/yyyy}")
                            .FontSize(8).FontColor("#7F8C8D");
                    });

                    row.ConstantItem(160).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Today:dd/MM/yyyy}").FontSize(7).FontColor("#7F8C8D");
                    });
                });

                // ── Contenido ─────────────────────────────────────
                page.Content().PaddingTop(10).Column(col =>
                {
                    // Tarjetas resumen
                    var totalDebe = vm.Asientos.Sum(a => a.TotalDebe);
                    var totalHaber = vm.Asientos.Sum(a => a.TotalHaber);

                    col.Item().PaddingBottom(12).Row(row =>
                    {
                        AgregarTarjeta(row, "Asientos Generados", vm.Asientos.Count.ToString(), "#346491");
                        row.ConstantItem(8);
                        AgregarTarjeta(row, "Total Debe", totalDebe.ToString("N2"), ColorDebe);
                        row.ConstantItem(8);
                        AgregarTarjeta(row, "Total Haber", totalHaber.ToString("N2"), ColorHaber);
                    });

                    // Asientos
                    foreach (var asiento in vm.Asientos)
                    {
                        // Encabezado asiento
                        col.Item().PaddingBottom(2).Row(row =>
                        {
                            row.RelativeItem().Background(ColorEncabezado)
                                .PaddingVertical(5).PaddingHorizontal(6)
                                .Column(c =>
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"Asiento N° {asiento.Numero}  —  {asiento.Glosa}")
                                            .Bold().FontSize(9).FontColor(Colors.White);
                                        r.ConstantItem(100).AlignRight()
                                            .Text(asiento.Fecha.ToString("dd/MM/yyyy"))
                                            .FontSize(8).FontColor(Colors.White);
                                    });
                                });
                        });

                        // Cabecera columnas
                        col.Item().Row(row => RenderHeaderTabla(row));

                        // Líneas
                        bool par = false;
                        foreach (var linea in asiento.Lineas)
                        {
                            var bg = par ? "#F5F5F5" : "#FFFFFF";
                            par = !par;
                            col.Item().BorderBottom(1).BorderColor("#E8E8E8").Background(bg).Row(row =>
                            {
                                row.ConstantItem(45).PaddingVertical(3).PaddingHorizontal(4)
                                    .Text(linea.CodigoCuenta).FontSize(8).FontColor("#7F8C8D");

                                row.RelativeItem().PaddingVertical(3).PaddingHorizontal(4)
                                    .Text(linea.NombreCuenta).FontSize(8);

                                row.ConstantItem(70).PaddingVertical(3).PaddingHorizontal(4).AlignRight()
                                    .Text(linea.Debe != 0 ? linea.Debe.ToString("N2") : "—")
                                    .FontSize(8)
                                    .FontColor(linea.Debe != 0 ? ColorDebe : "#AAAAAA");

                                row.ConstantItem(70).PaddingVertical(3).PaddingHorizontal(4).AlignRight()
                                    .Text(linea.Haber != 0 ? linea.Haber.ToString("N2") : "—")
                                    .FontSize(8)
                                    .FontColor(linea.Haber != 0 ? ColorHaber : "#AAAAAA");
                            });
                        }

                        // Fila de totales
                        col.Item().Background("#F0F0F0").Row(row =>
                        {
                            row.ConstantItem(45);
                            row.RelativeItem().PaddingVertical(3).PaddingHorizontal(4)
                                .Text("TOTALES").Bold().FontSize(8);
                            row.ConstantItem(70).PaddingVertical(3).PaddingHorizontal(4).AlignRight()
                                .Text(asiento.TotalDebe.ToString("N2")).Bold().FontSize(8).FontColor(ColorDebe);
                            row.ConstantItem(70).PaddingVertical(3).PaddingHorizontal(4).AlignRight()
                                .Text(asiento.TotalHaber.ToString("N2")).Bold().FontSize(8).FontColor(ColorHaber);
                        });

                        col.Item().Height(12);
                    }
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

    private static void AgregarTarjeta(RowDescriptor row, string label, string valor, string color)
    {
        row.RelativeItem().Border(1).BorderColor(color).Column(col =>
        {
            col.Item().Background(color)
                .PaddingVertical(4).PaddingHorizontal(6)
                .AlignCenter()
                .Text(label).Bold().FontSize(7).FontColor(Colors.White);

            col.Item().PaddingVertical(5).PaddingHorizontal(6)
                .AlignCenter()
                .Text(valor).Bold().FontSize(10);
        });
    }

    private static void RenderHeaderTabla(RowDescriptor row)
    {
        row.ConstantItem(45).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4)
            .Text("Código").Bold().FontSize(7).FontColor(Colors.White);

        row.RelativeItem().Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4)
            .Text("Cuenta").Bold().FontSize(7).FontColor(Colors.White);

        row.ConstantItem(70).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4).AlignRight()
            .Text("Debe").Bold().FontSize(7).FontColor(Colors.White);

        row.ConstantItem(70).Background(ColorHeaderTabla)
            .PaddingVertical(3).PaddingHorizontal(4).AlignRight()
            .Text("Haber").Bold().FontSize(7).FontColor(Colors.White);
    }
}
