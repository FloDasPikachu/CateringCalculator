using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class EventPlannerResult {
    [Inject]
    private FileSaveService FileSaveService { get; set; } = default!;
    [Inject]
    private IRecipeRepository RecipeRepository { get; set; } = default!;

    [Parameter]
    public CalculationResult? CalculationResult { get; set; }
    [Parameter]
    public EventCallback OnStockDeducted { get; set; }

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

    private async Task DeductStock() {
        if (CalculationResult == null)
            return;

        foreach (var item in CalculationResult.ShoppingList) {
            if (!item.IsStockUsed)
                continue;

            var ingredients = await RecipeRepository.GetAllIngredientsAsync();
            var ingredient = ingredients.FirstOrDefault(i => i.Id == item.IngredientId);

            if (ingredient != null) {
                ingredient.StockAmount = Math.Max(0m, ingredient.StockAmount - item.TotalAmountNeeded);
                await RecipeRepository.UpdateIngredientAsync(ingredient);
            }
        }

        if (OnStockDeducted.HasDelegate) {
            await OnStockDeducted.InvokeAsync();
        }
    }

    private static string GetUnitSuffix(IngredientUnit unit) => unit switch {
        IngredientUnit.Piece => "Stk.",
        IngredientUnit.Gram => "g",
        IngredientUnit.Milliliter => "ml",
        _ => "ml"
    };
}