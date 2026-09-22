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

        var ingredientVolumes = new Dictionary<Guid, (Ingredient Ingredient, decimal TotalVolumeMl)>();
        decimal totalIceGrams = 0;

        for (int i = 0; i < eventPlan.SelectedRecipes.Count; i++) {
            var recipe = eventPlan.SelectedRecipes[i];
            int countForThisRecipe = drinksPerRecipe + (i == 0 ? remainingDrinks : 0);

            totalIceGrams += recipe.IceInGrams * countForThisRecipe;

            foreach (var item in recipe.Items) {
                if (item.Ingredient == null) {
                    continue;
                }

                decimal volumeNeeded = item.AmountMl * countForThisRecipe;

                if (ingredientVolumes.ContainsKey(item.Ingredient.Id)) {
                    var current = ingredientVolumes[item.Ingredient.Id];
                    ingredientVolumes[item.Ingredient.Id] = (current.Ingredient, current.TotalVolumeMl + volumeNeeded);
                } else {
                    ingredientVolumes[item.Ingredient.Id] = (item.Ingredient, volumeNeeded);
                }
            }
        }

        decimal wasteMultiplier = 1m + (eventPlan.WasteBufferPercent / 100m);

        foreach (var entry in ingredientVolumes.Values) {
            var ingredient = entry.Ingredient;
            decimal totalMlWithWaste = entry.TotalVolumeMl * wasteMultiplier;

            int bottlesToBuy = 0;
            if (ingredient.BottleVolumeMl > 0) {
                bottlesToBuy = (int)Math.Ceiling(totalMlWithWaste / ingredient.BottleVolumeMl);
            }

            result.ShoppingList.Add(new IngredientShoppingItem {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                TotalVolumeNeededMl = Math.Round(totalMlWithWaste, 2),
                BottleVolumeMl = ingredient.BottleVolumeMl,
                BottlePrice = ingredient.BottlePrice,
                BottlesToBuy = bottlesToBuy
            });
        }

        result.TotalIceInKg = Math.Round((totalIceGrams * wasteMultiplier) / 1000m, 2);

        return result;
    }
}