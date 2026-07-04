using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public class LibroMayorPdfService : ILibroMayorPdfService
{
    private static readonly string ColorEncabezado = "#34495E";
    private static readonly string ColorHeaderTabla = "#2C3E50";
    private static readonly string ColorFilaPar = "#F5F5F5";
    private static readonly string ColorSaldoInicial = "#4A90D9";
    private static readonly string ColorDebitos = "#27AE60";
    private static readonly string ColorCreditos = "#E74C3C";
    private static readonly string ColorSaldoFinal = "#2980B9";

    public byte[] Generar(LibroMayorViewModel vm)
    {
        var documento = Document.Create(container =>
        {
            foreach (var cuenta in vm.CuentasMayor)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    // ── Header ───────────────────────────────────────
                    page.Header().BorderBottom(1).BorderColor(ColorEncabezado).PaddingBottom(4).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Libro Mayor").Bold().FontSize(10).FontColor(ColorEncabezado);
                            if (!string.IsNullOrWhiteSpace(vm.ClienteNombre))
                                col.Item().Text(vm.ClienteNombre).FontSize(8).FontColor("#7F8C8D");
                        });

                        row.ConstantItem(200).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Generado: {DateTime.Today:dd/MM/yyyy}").FontSize(7).FontColor("#7F8C8D");
                            if (vm.FechaDesde.HasValue || vm.FechaHasta.HasValue)
                            {
                                var periodo = $"Período: {vm.FechaDesde?.ToString("dd/MM/yyyy") ?? "–"} al {vm.FechaHasta?.ToString("dd/MM/yyyy") ?? "–"}";
                                col.Item().Text(periodo).FontSize(7).FontColor("#7F8C8D");
                            }
                        });
                    });

                    // ── Contenido ─────────────────────────────────────
                    page.Content().PaddingTop(8).Column(col =>
                    {
                        // Título cuenta
                        col.Item().Text($"{cuenta.Codigo} – {cuenta.Nombre}")
                            .Bold().FontSize(12).FontColor(ColorEncabezado);

                        col.Item().PaddingTop(2).Text($"Tipo: {cuenta.Tipo}   |   Naturaleza: {cuenta.Naturaleza}")
                            .Italic().FontSize(8).FontColor("#7F8C8D");

                        col.Item().PaddingTop(10).PaddingBottom(10).Row(resumen =>
                        {
                            AgregarTarjetaResumen(resumen, "Saldo inicial (UYU)", cuenta.SaldoInicialBase, ColorSaldoInicial);
                            resumen.ConstantItem(8);
                            AgregarTarjetaResumen(resumen, "Débitos (UYU)", cuenta.DebitosBase, ColorDebitos);
                            resumen.ConstantItem(8);
                            AgregarTarjetaResumen(resumen, "Créditos (UYU)", cuenta.CreditosBase, ColorCreditos);
                            resumen.ConstantItem(8);

                            string saldoFinalLabel;
                            decimal saldoFinalValor;
                            if (cuenta.DebitosBase > cuenta.CreditosBase) {
                                saldoFinalLabel = "Saldo deudor (UYU)";
                                saldoFinalValor = cuenta.DebitosBase - cuenta.CreditosBase;
                            } else if (cuenta.CreditosBase > cuenta.DebitosBase) {
                                saldoFinalLabel = "Saldo acreedor (UYU)";
                                saldoFinalValor = cuenta.CreditosBase - cuenta.DebitosBase;
                            } else {
                                saldoFinalLabel = "Saldo deudor (UYU)";
                                saldoFinalValor = 0m;
                            }

                            AgregarTarjetaResumen(resumen, saldoFinalLabel, saldoFinalValor, ColorSaldoFinal);
                        });

                        // Tabla movimientos
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(60);  // Fecha
                                cols.ConstantColumn(55);  // Asiento
                                cols.RelativeColumn();    // Glosa
                                cols.ConstantColumn(65);  // Debe
                                cols.ConstantColumn(65);  // Haber
                                cols.ConstantColumn(65);  // Saldo
                                cols.ConstantColumn(40);  // Moneda
                                cols.ConstantColumn(65);  // Saldo base
                            });

                            // Header tabla
                            tabla.Header(h =>
                            {
                                var headers = new[] { "Fecha", "Asiento", "Glosa", "Debe", "Haber", "Saldo", "Moneda", "Saldo base" };
                                foreach (var (header, idx) in headers.Select((hdr, i) => (hdr, i)))
                                {
                                    var esNumerico = idx >= 3 && idx != 6;
                                    h.Cell().Background(ColorHeaderTabla)
                                        .PaddingVertical(4).PaddingHorizontal(4)
                                        .AlignMiddle()
                                        .Element(e => esNumerico ? e.AlignRight() : e.AlignLeft())
                                        .Text(header).Bold().FontColor(Colors.White).FontSize(7);
                                }
                            });

                            // Filas
                            int filaIdx = 0;
                            foreach (var mov in cuenta.Movimientos)
                            {
                                var bg = filaIdx % 2 == 0 ? "#FFFFFF" : ColorFilaPar;

                                CeldaTabla(tabla, mov.Fecha.ToString("dd/MM/yyyy"), bg, false);
                                CeldaTabla(tabla, mov.NumeroAsiento.ToString(), bg, false);
                                CeldaTabla(tabla, mov.Glosa ?? string.Empty, bg, false);
                                CeldaTabla(tabla, mov.Debe.ToString("N2"), bg, true);
                                CeldaTabla(tabla, mov.Haber.ToString("N2"), bg, true);
                                CeldaTabla(tabla, mov.SaldoAcumulado.ToString("N2"), bg, true);
                                CeldaTabla(tabla, mov.Moneda ?? string.Empty, bg, false, center: true);
                                CeldaTabla(tabla, mov.SaldoAcumuladoBase.ToString("N2"), bg, true);

                                filaIdx++;
                            }
                        });
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
            }
        });

        return documento.GeneratePdf();
    }

    private static void AgregarTarjetaResumen(RowDescriptor row, string label, decimal valor, string color)
    {
        row.RelativeItem().Border(1).BorderColor(color).Column(col =>
        {
            col.Item()
                .Background(color)
                .PaddingVertical(4).PaddingHorizontal(6)
                .AlignCenter()
                .Text(label).Bold().FontSize(7).FontColor(Colors.White);

            col.Item()
                .PaddingVertical(5).PaddingHorizontal(6)
                .AlignCenter()
                .Text(valor.ToString("N2")).Bold().FontSize(9);
        });
    }

    private static void CeldaTabla(TableDescriptor tabla, string valor, string bg, bool alinearDerecha, bool center = false)
    {
        var celda = tabla.Cell()
            .Background(bg)
            .BorderBottom(1).BorderColor("#E0E0E0")
            .PaddingVertical(3).PaddingHorizontal(4);

        var alineada = alinearDerecha ? celda.AlignRight()
                     : center ? celda.AlignCenter()
                     : celda.AlignLeft();

        alineada.Text(valor).FontSize(7);
    }
}