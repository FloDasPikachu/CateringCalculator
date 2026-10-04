using CateringCalculator.Core.Enums;

namespace CateringCalculator.Core.Helpers;

public static class FormattingHelper {
    public static string FormatAmount(decimal amount, IngredientUnit unit) {
        return unit switch {
            IngredientUnit.Piece => $"{amount:0.##} Stk.",
            IngredientUnit.Gram => amount >= 1000m ? $"{(amount / 1000m):0.##} kg" : $"{amount:0.##} g",
            IngredientUnit.Milliliter => amount >= 1000m ? $"{(amount / 1000m):0.##} l" : $"{amount:0.##} ml",
            _ => $"{amount:0.##}"
        };
    }
}