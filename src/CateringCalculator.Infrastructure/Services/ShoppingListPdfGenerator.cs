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
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                // Header
                page.Header().Column(column => {
                    column.Item().Text($"Ergebnis: {result.EventTitle}")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);

                    var subtitle = $"{result.TotalDrinksCount} zahlende Drinks";
                    if (result.FreeDrinksCount > 0) {
                        subtitle += $" (+ {result.FreeDrinksCount} Freigetränke)";
                    }

                    column.Item().PaddingTop(2).Text(subtitle)
                        .FontSize(11).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(5).Column(column => {
                    // 1. Kennzahlen-Kacheln (4er-Grid passend zur UI)
                    column.Item().PaddingBottom(15).Column(kBox => {
                        kBox.Item().PaddingBottom(8).Row(row => {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c => {
                                c.Item().Text("Wareneinsatz (Einkauf)").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"{result.TotalMaterialCost:C2}").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c => {
                                c.Item().Text("Gesamtkosten (inkl. Fix/Pers)").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"{result.TotalEventCosts:C2}").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                            });
                        });

                        kBox.Item().Row(row => {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c => {
                                c.Item().Text("Ziel-Umsatz (inkl. Gewinn)").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"{result.TotalTargetRevenue:C2}").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c => {
                                c.Item().Text("Ø Verkaufspreis / Drink").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"{result.TargetSalesPricePerDrink:C2}").FontSize(12).Bold().FontColor(Colors.Purple.Darken2);
                            });
                        });
                    });

                    // 2. Kalkulation pro Cocktail & Verkaufspreis
                    column.Item().PaddingBottom(5).Text("Cocktail-Kalkulation & Verkaufspreise").FontSize(12).Bold();

                    column.Item().PaddingBottom(15).Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.RelativeColumn(2.5f); // Cocktail
                            columns.RelativeColumn(1f);   // Anteil
                            columns.RelativeColumn(1f);   // Anzahl
                            columns.RelativeColumn(1.8f); // Real / Drink
                            columns.RelativeColumn(2.2f); // Empf. Verkaufspreis
                        });

                        table.Header(header => {
                            header.Cell().Element(HeaderStyle).Text("Cocktail");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("Anteil");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("Anzahl");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Real / Drink*");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Empf. Preis");
                        });

                        foreach (var calc in result.RecipeCalculations) {
                            table.Cell().Element(CellStyle).Text(calc.RecipeName).Bold();
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{calc.Percentage:0.##} %");
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{calc.PayingDrinkCount} ({calc.FreeDrinkCount}) Stk.");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{calc.RealCostPerDrink:C2}").FontColor(Colors.Green.Darken2);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{calc.TargetSalesPrice:C2}").Bold().FontColor(Colors.Purple.Darken2);
                        }
                    });

                    // 3. Automatische Einkaufsliste
                    column.Item().PaddingBottom(5).Text("Automatische Einkaufsliste").FontSize(12).Bold();

                    column.Item().Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(20);  // Checkbox [ ]
                            columns.RelativeColumn(3);   // Zutat
                            columns.RelativeColumn(2);   // Gesamtbedarf
                            columns.RelativeColumn(3);   // Kaufm. Gebinde
                            columns.RelativeColumn(2);   // Gesamtkosten
                        });

                        table.Header(header => {
                            header.Cell().Element(HeaderStyle).Text("[x]");
                            header.Cell().Element(HeaderStyle).Text("Zutat");
                            header.Cell().Element(HeaderStyle).Text("Gesamtbedarf");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("Kaufm. Gebinde");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Gesamtkosten");
                        });

                        foreach (var item in result.ShoppingList) {
                            table.Cell().Element(CellStyle).Text("[  ]");
                            table.Cell().Element(CellStyle).Text(item.IngredientName).Bold();
                            table.Cell().Element(CellStyle).Text(FormatAmount(item.TotalAmountNeeded, item.Unit));
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.PackagesToBuy} x ({FormatPackageSize(item.PackageSize, item.Unit)})");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalCost:C2}").Bold();
                        }
                    });
                });

                // Footer
                page.Footer().AlignRight().Text(x => {
                    x.Span("Erstellt am ");
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    x.Span(" Uhr");
                });
            });
        });

        return document.GeneratePdf();

        static IContainer HeaderStyle(IContainer container) =>
            container.Background(Colors.Grey.Lighten3)
                     .Padding(5)
                     .DefaultTextStyle(x => x.Bold().FontSize(8.5f));

        static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten3)
                     .Padding(5)
                     .DefaultTextStyle(x => x.FontSize(8.5f));
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

    private static string FormatPackageSize(decimal packageSize, IngredientUnit unit) {
        return unit switch {
            IngredientUnit.Piece => $"{packageSize:0.##} Stk.",
            IngredientUnit.Gram => packageSize >= 1000m
                ? $"{(packageSize / 1000m):0.##} kg"
                : $"{packageSize:0.##} g",
            IngredientUnit.Milliliter => packageSize >= 1000m
                ? $"{(packageSize / 1000m):0.##} l"
                : $"{packageSize:0.##} ml",
            _ => $"{packageSize:0.##}"
        };
    }
}