using CateringCalculator.Core.Enums;
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

                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                // Tabelle mit Zutaten und Checkboxen
                page.Content().PaddingVertical(10).Column(column => {
                    column.Item().Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(30); // [x] Checkbox
                            columns.RelativeColumn(3);  // Zutat
                            columns.RelativeColumn(2);  // Bedarf
                            columns.RelativeColumn(2.5f); // Kaufm. Gebinde
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
                            table.Cell().Element(CellStyle).Text("[  ]");
                            table.Cell().Element(CellStyle).Text(item.IngredientName).Bold();
                            table.Cell().Element(CellStyle).Text(FormatAmount(item.TotalAmountNeeded, item.Unit));
                            table.Cell().Element(CellStyle).Text(FormatPackage(item));
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalCost:C2}");

                            static IContainer CellStyle(IContainer container) =>
                                container.BorderBottom(1)
                                         .BorderColor(Colors.Grey.Lighten3)
                                         .Padding(5);
                        }
                    });

                    // Kennzahlen unten
                    column.Item().PaddingTop(15).Row(row => {
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

    private static string FormatAmount(decimal amount, IngredientUnit unit) {
        return unit switch {
            IngredientUnit.Piece => $"{amount:0.##} Stk.",
            IngredientUnit.Gram => amount >= 1000m
                ? $"{(amount / 1000m):0.##} kg"
                : $"{amount:0.##} g",
            IngredientUnit.Milliliter => amount >= 1000m
                ? $"{(amount / 1000m):0.##} l"
                : $"{amount:0.##} ml",
            _ => $"{amount:0.##}"
        };
    }

    private static string FormatPackage(IngredientShoppingItem item) {
        string packageText = item.Unit switch {
            IngredientUnit.Piece => $"{item.PackageSize:0.##} Stk. Packung",
            IngredientUnit.Gram => item.PackageSize >= 1000m
                ? $"{(item.PackageSize / 1000m):0.##} kg Packung"
                : $"{item.PackageSize:0.##} g Packung",
            IngredientUnit.Milliliter => item.PackageSize >= 1000m
                ? $"{(item.PackageSize / 1000m):0.##} l Flasche"
                : $"{item.PackageSize:0.##} ml Flasche",
            _ => $"{item.PackageSize:0.##}"
        };

        return $"{item.PackagesToBuy}x ({packageText})";
    }
}