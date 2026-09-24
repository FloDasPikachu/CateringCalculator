using Microsoft.AspNetCore.Components;
using CateringCalculator.Core.Models;
using CateringCalculator.Core.Enums;

namespace CateringCalculator.UI.Components; // Passe den Namespace bei Bedarf an dein Projekt an!

public partial class RecipeCard : ComponentBase {
    [Parameter, EditorRequired]
    public Recipe Recipe { get; set; } = default!;

    [Parameter]
    public EventCallback<Recipe> OnEdit { get; set; }

    [Parameter]
    public EventCallback<Recipe> OnDelete { get; set; }

    [Parameter]
    public EventCallback<string> OnGroupClick { get; set; }

    protected static string GetUnitSuffix(IngredientUnit unit) => unit switch {
        IngredientUnit.Piece => "Stk.",
        IngredientUnit.Gram => "g",
        IngredientUnit.Milliliter => "ml",
        _ => "ml"
    };

    protected static string FormatAmount(decimal amount, IngredientUnit? unit) {
        var suffix = unit.HasValue ? GetUnitSuffix(unit.Value) : "ml";
        return $"{amount:0.##} {suffix}";
    }
}