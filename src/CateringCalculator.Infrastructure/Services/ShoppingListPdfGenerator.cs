using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace CateringCalculator.Infrastructure.Services;

public class ShoppingListPdfGenerator {
    private static bool _isInitialized = false;

    // Statischer Konstruktor
    static ShoppingListPdfGenerator() {
        InitializeFontResolver();
    }

    private static void InitializeFontResolver() {
        if (_isInitialized)
            return;

        try {
            // Manchmal wirft schon das Abfragen von GlobalFontSettings.FontResolver den Fehler,
            // wenn noch kein Resolver gesetzt ist. Wir setzen ihn daher direkt per Zuweisung.
            GlobalFontSettings.FontResolver = new CustomFontResolver();
            _isInitialized = true;
        } catch (Exception) {
            // Falls er bereits gesetzt wurde oder ein interner Konflikt auftritt, ignorieren
            _isInitialized = true;
        }
    }

    public static byte[] GeneratePdf(CalculationResult result) {
        // Zur Sicherheit vor jedem PDF-Generieren nochmal aufrufen
        InitializeFontResolver();

        var document = new PdfDocument();
        document.Info.Title = $"Einkaufsliste - {result.EventTitle}";

        // Seite hinzufügen (A4 Hochformat)
        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        var gfx = XGraphics.FromPdfPage(page);

        // Farben definieren
        var colPrimary = XColor.FromArgb(20, 50, 100); // Dunkelblau
        var colGrayText = XColor.FromArgb(100, 100, 100);
        var colLightGray = XColor.FromArgb(240, 240, 240);
        var colGreen = XColor.FromArgb(0, 120, 50);
        var colRed = XColor.FromArgb(180, 40, 40);
        var colPurple = XColor.FromArgb(110, 40, 140);

        // Fonts definieren
        var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
        var fontSubtitle = new XFont("Arial", 10, XFontStyle.Regular);
        var fontSection = new XFont("Arial", 12, XFontStyle.Bold);
        var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
        var fontBody = new XFont("Arial", 9, XFontStyle.Regular);
        var fontBodyBold = new XFont("Arial", 9, XFontStyle.Bold);

        // Standard-Format für Textboxen (verhindert den Baseline-Crash)
        var formatLeft = new XStringFormat { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.Near };

        double margin = 40; // Seitenrand in Punkten
        double yPos = margin;
        double usableWidth = page.Width - (2 * margin);

        // --- 1. HEADER ---
        gfx.DrawString($"Ergebnis: {result.EventTitle}", fontTitle, new XSolidBrush(colPrimary), new XRect(margin, yPos, usableWidth, 25), formatLeft);
        yPos += 22;

        var subtitle = $"{result.TotalDrinksCount} zahlende Drinks";
        if (result.FreeDrinksCount > 0) {
            subtitle += $" (+ {result.FreeDrinksCount} Freigetränke)";
        }
        gfx.DrawString(subtitle, fontSubtitle, new XSolidBrush(colGrayText), new XRect(margin, yPos, usableWidth, 18), formatLeft);
        yPos += 20;

        // Trennlinie
        gfx.DrawLine(new XPen(XColors.LightGray, 1), margin, yPos, page.Width - margin, yPos);
        yPos += 15;

        // --- 2. KENNZAHLEN (Kacheln) ---
        double boxWidth = (usableWidth - 10) / 2;
        double boxHeight = 35;

        // Funktion zum Zeichnen einer Kennzahlen-Box
        void DrawKpiBox(double x, double y, string label, string value, XColor valueColor) {
            gfx.DrawRectangle(new XSolidBrush(colLightGray), x, y, boxWidth, boxHeight);
            gfx.DrawRectangle(new XPen(XColors.LightGray, 1), x, y, boxWidth, boxHeight);

            gfx.DrawString(label, fontBody, new XSolidBrush(colGrayText), new XRect(x + 5, y + 4, boxWidth - 10, 12), formatLeft);
            gfx.DrawString(value, fontBodyBold, new XSolidBrush(valueColor), new XRect(x + 5, y + 18, boxWidth - 10, 15), formatLeft);
        }

        DrawKpiBox(margin, yPos, "Wareneinsatz (Einkauf)", $"{result.TotalMaterialCost:C2}", colGreen);
        DrawKpiBox(margin + boxWidth + 10, yPos, "Gesamtkosten (inkl. Fix/Pers)", $"{result.TotalEventCosts:C2}", colRed);
        yPos += boxHeight + 8;

        DrawKpiBox(margin, yPos, "Ziel-Umsatz (inkl. Gewinn)", $"{result.TotalTargetRevenue:C2}", colPrimary);
        DrawKpiBox(margin + boxWidth + 10, yPos, "Ø Verkaufspreis / Drink", $"{result.TargetSalesPricePerDrink:C2}", colPurple);
        yPos += boxHeight + 20;

        // --- 3. COCKTAIL-KALKULATION ---
        gfx.DrawString("Cocktail-Kalkulation & Verkaufspreise", fontSection, XBrushes.Black, new XRect(margin, yPos, usableWidth, 20), formatLeft);
        yPos += 22;

        // Tabellenkopf
        double[] colWidths1 = { 120, 50, 60, 95, 95 };
        DrawTableHeader(gfx, fontHeader, colLightGray, margin, ref yPos, usableWidth, colWidths1, ["Cocktail", "Anteil", "Anzahl", "Real / Drink", "Empf. Preis"]);

        foreach (var calc in result.RecipeCalculations) {
            DrawTableRow(gfx, fontBody, margin, ref yPos, usableWidth, colWidths1, [
                calc.RecipeName,
                $"{calc.Percentage:0.##} %",
                $"{calc.PayingDrinkCount} ({calc.FreeDrinkCount})",
                $"{calc.RealCostPerDrink:C2}",
                $"{calc.TargetSalesPrice:C2}"
            ]);
        }

        yPos += 15;

        // --- 4. EINKAUFSLISTE ---
        gfx.DrawString("Automatische Einkaufsliste", fontSection, XBrushes.Black, new XRect(margin, yPos, usableWidth, 20), formatLeft);
        yPos += 22;

        double[] colWidths2 = { 25, 130, 90, 115, 60 };
        DrawTableHeader(gfx, fontHeader, colLightGray, margin, ref yPos, usableWidth, colWidths2, ["[ ]", "Zutat", "Gesamtbedarf", "Kaufm. Gebinde", "Kosten"]);

        foreach (var item in result.ShoppingList) {
            DrawTableRow(gfx, fontBody, margin, ref yPos, usableWidth, colWidths2, [
                "[  ]",
                item.IngredientName,
                FormatAmount(item.TotalAmountNeeded, item.Unit),
                $"{item.PackagesToBuy} x ({FormatPackageSize(item.PackageSize, item.Unit)})",
                $"{item.TotalCost:C2}"
            ]);
        }

        // Speicher-Stream generieren
        using var memoryStream = new MemoryStream();
        document.Save(memoryStream);
        return memoryStream.ToArray();
    }

    private static void DrawTableHeader(XGraphics gfx, XFont font, XColor bgColor, double startX, ref double yPos, double totalWidth, double[] colWidths, string[] headers) {
        double currentX = startX;
        double height = 18;

        gfx.DrawRectangle(new XSolidBrush(bgColor), startX, yPos, totalWidth, height);

        for (int i = 0; i < headers.Length; i++) {
            var rect = new XRect(currentX + 3, yPos + 3, colWidths[i] - 6, height - 3);
            var format = new XStringFormat {
                Alignment = i >= 3 ? XStringAlignment.Far : (i == 1 && headers[i] == "Anteil" ? XStringAlignment.Center : XStringAlignment.Near),
                LineAlignment = XLineAlignment.Near
            };

            gfx.DrawString(headers[i], font, XBrushes.Black, rect, format);
            currentX += colWidths[i];
        }
        yPos += height;
    }

    private static void DrawTableRow(XGraphics gfx, XFont font, double startX, ref double yPos, double totalWidth, double[] colWidths, string[] values) {
        double currentX = startX;
        double height = 18;

        // Untere Trennlinie für Zeile
        gfx.DrawLine(new XPen(XColors.LightGray, 0.5), startX, yPos + height, startX + totalWidth, yPos + height);

        for (int i = 0; i < values.Length; i++) {
            var rect = new XRect(currentX + 3, yPos + 3, colWidths[i] - 6, height - 3);
            var format = new XStringFormat {
                Alignment = i >= 3 ? XStringAlignment.Far : XStringAlignment.Near,
                LineAlignment = XLineAlignment.Near
            };

            gfx.DrawString(values[i], font, XBrushes.DarkSlateGray, rect, format);
            currentX += colWidths[i];
        }
        yPos += height;
    }

    private static string FormatAmount(decimal amount, IngredientUnit unit) {
        return unit switch {
            IngredientUnit.Piece => $"{amount:0.##} Stk.",
            IngredientUnit.Gram => amount >= 1000m ? $"{(amount / 1000m):0.##} kg" : $"{amount:0.##} g",
            IngredientUnit.Milliliter => amount >= 1000m ? $"{(amount / 1000m):0.##} l" : $"{amount:0.##} ml",
            _ => $"{amount:0.##}"
        };
    }

    private static string FormatPackageSize(decimal packageSize, IngredientUnit unit) {
        return unit switch {
            IngredientUnit.Piece => $"{packageSize:0.##} Stk.",
            IngredientUnit.Gram => packageSize >= 1000m ? $"{(packageSize / 1000m):0.##} kg" : $"{packageSize:0.##} g",
            IngredientUnit.Milliliter => packageSize >= 1000m ? $"{(packageSize / 1000m):0.##} l" : $"{packageSize:0.##} ml",
            _ => $"{packageSize:0.##}"
        };
    }
}