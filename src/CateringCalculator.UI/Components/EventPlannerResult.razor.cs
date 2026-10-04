using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components; 
public partial class EventPlannerResult {
    [Inject]
    private FileSaveService FileSaveService { get; set; } = default!;

    [Parameter]
    public CalculationResult? CalculationResult { get; set; }

    private async Task ExportPdf() {
        if (CalculationResult == null)
            return;

        try {
            var pdfBytes = CateringCalculator.Infrastructure.Services.ShoppingListPdfGenerator.GeneratePdf(CalculationResult);
            var fileName = $"Einkaufsliste_{CalculationResult.EventTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            await FileSaveService.SaveAndOpenFileAsync(fileName, pdfBytes);
        } catch (Exception) {
            throw;
        }
    }

    private static string GetUnitSuffix(IngredientUnit unit) => unit switch {
        IngredientUnit.Piece => "Stk.",
        IngredientUnit.Gram => "g",
        IngredientUnit.Milliliter => "ml",
        _ => "ml"
    };
}
