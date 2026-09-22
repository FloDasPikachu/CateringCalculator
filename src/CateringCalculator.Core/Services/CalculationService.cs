using CateringCalculator.Core.Models;

namespace CateringCalculator.Core.Services;

public class CalculationService {
    public CalculationResult Calculate(EventPlan eventPlan) {
        ArgumentNullException.ThrowIfNull(eventPlan);

        var result = new CalculationResult {
            EventPlanId = eventPlan.Id,
            EventTitle = eventPlan.Title,
            TotalDrinksCount = eventPlan.TotalDrinksToServe
        };

        if (eventPlan.SelectedRecipes.Count == 0 || eventPlan.TotalDrinksToServe == 0) {
            return result;
        }

        int drinksPerRecipe = eventPlan.TotalDrinksToServe / eventPlan.SelectedRecipes.Count;
        int remainingDrinks = eventPlan.TotalDrinksToServe % eventPlan.SelectedRecipes.Count;

        var ingredientAmounts = new Dictionary<Guid, (Ingredient Ingredient, decimal TotalAmount)>();
        decimal totalIceGrams = 0;

        for (int i = 0; i < eventPlan.SelectedRecipes.Count; i++) {
            var recipe = eventPlan.SelectedRecipes[i];
            int countForThisRecipe = drinksPerRecipe + (i == 0 ? remainingDrinks : 0);

            totalIceGrams += recipe.IceInGrams * countForThisRecipe;

            foreach (var item in recipe.Items) {
                if (item.Ingredient == null) {
                    continue;
                }

                decimal amountNeeded = item.Amount * countForThisRecipe;

                if (ingredientAmounts.TryGetValue(item.Ingredient.Id, out (Ingredient Ingredient, decimal TotalAmount) current)) {
                    ingredientAmounts[item.Ingredient.Id] = (current.Ingredient, current.TotalAmount + amountNeeded);
                } else {
                    ingredientAmounts[item.Ingredient.Id] = (item.Ingredient, amountNeeded);
                }
            }
        }

        decimal wasteMultiplier = 1m + (eventPlan.WasteBufferPercent / 100m);

        foreach (var entry in ingredientAmounts.Values) {
            var ingredient = entry.Ingredient;
            decimal totalMlWithWaste = entry.TotalAmount * wasteMultiplier;

            int bottlesToBuy = 0;
            if (ingredient.PackageSize > 0) {
                bottlesToBuy = (int)Math.Ceiling(totalMlWithWaste / ingredient.PackageSize);
            }

            result.ShoppingList.Add(new IngredientShoppingItem {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Unit = ingredient.Unit,
                TotalAmountNeeded = Math.Round(totalMlWithWaste, 2),
                PackageSize = ingredient.PackageSize,
                PackagePrice = ingredient.PackagePrice,
                PackagesToBuy = bottlesToBuy
            });
        }

        result.TotalIceInKg = Math.Round((totalIceGrams * wasteMultiplier) / 1000m, 2);

        return result;
    }
}