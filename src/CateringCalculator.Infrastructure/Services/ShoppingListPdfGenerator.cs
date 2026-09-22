using CateringCalculator.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CateringCalculator.Infrastructure.Services;

public class ShoppingListPdfGenerator {
    static ShoppingListPdfGenerator() {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GeneratePdf(CalculationResult result) {
        var document = Document.Create(container => {
            container.Page(page => {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Lato"));

                // Header
                page.Header().Column(column => {
                    column.Item().Text("Catering Calculator - Einkaufsliste")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(5).Text($"Veranstaltung: {result.EventTitle}")
                        .FontSize(14).SemiBold();

                    column.Item().Text($"Berechnet für {result.TotalDrinksCount} Drinks gesamt | Eisbedarf: {result.TotalIceInKg} kg")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                // Tabelle mit Zutaten und Checkboxen
                page.Content().PaddingVertical(10).Column(column => {
                    column.Item().Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(30); // Checkbox [ ]
                            columns.RelativeColumn(3);  // Zutat
                            columns.RelativeColumn(2);  // Bedarf (ml)
                            columns.RelativeColumn(2);  // Kaufm. Gebinde
                            columns.RelativeColumn(2);  // Gesamtkosten
                        });

                        // Tabellen-Header
                        table.Header(header => {
                            header.Cell().Element(HeaderStyle).Text("[x]");
                            header.Cell().Element(HeaderStyle).Text("Zutat");
                            header.Cell().Element(HeaderStyle).Text("Bedarf");
                            header.Cell().Element(HeaderStyle).Text("Einkauf (Gebinde)");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Kosten");

                            static IContainer HeaderStyle(IContainer container) =>
                                container.Background(Colors.Grey.Lighten2)
                                         .Padding(5)
                                         .DefaultTextStyle(x => x.Bold());
                        });

                        // Datenzeilen
                        foreach (var item in result.ShoppingList) {
                            table.Cell().Element(CellStyle).Text("[  ]"); // Checkbox zum Abhaken
                            table.Cell().Element(CellStyle).Text(item.IngredientName).Bold();
                            table.Cell().Element(CellStyle).Text($"{item.TotalVolumeNeededMl} ml");
                            table.Cell().Element(CellStyle).Text($"{item.BottlesToBuy}x ({item.BottleVolumeMl} ml)");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalCost:C2}");

                            static IContainer CellStyle(IContainer container) =>
                                container.BorderBottom(1)
                                         .BorderColor(Colors.Grey.Lighten3)
                                         .Padding(5);
                        }
                    });

                    // Kennzahlen unten
                    column.Item().PaddingTop(15).Row(row => {
                        row.RelativeItem().Text($"Eisbedarf gesamt: {result.TotalIceInKg} kg")
                            .FontSize(11).Bold().FontColor(Colors.Blue.Darken2);

                        row.RelativeItem().AlignRight().Text($"Gesamtkosten: {result.TotalMaterialCost:C2}")
                            .FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                    });
                });

                // Footer
                page.Footer().AlignRight().Text(x => {
                    x.Span("Erstellt am ");
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }
}