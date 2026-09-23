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

        var ingredientAmounts = new Dictionary<Guid, (Ingredient Ingredient, decimal TotalAmount)>();
        decimal totalIceGrams = 0;
        decimal wasteMultiplier = 1m + (eventPlan.WasteBufferPercent / 100m);

        // Map zur Speicherung des verbrauchten Bedarfs pro Rezept & Zutat
        // Key: RecipeId -> Value: (IngredientId -> Verbrauchte Menge inkl. Puffer)
        var recipeIngredientUsage = new Dictionary<Guid, Dictionary<Guid, decimal>>();

        // 1. Zutatenmengen & Soll-Kosten berechnen
        foreach (var eventRecipe in eventPlan.SelectedRecipes) {
            var recipe = eventRecipe.Recipe;
            if (recipe == null)
                continue;

            int countForThisRecipe = (int)Math.Round(eventPlan.TotalDrinksToServe * (eventRecipe.Percentage / 100m));
            totalIceGrams += recipe.IceInGrams * countForThisRecipe;

            decimal recipeTheoreticalCost = 0m;
            var usageDict = new Dictionary<Guid, decimal>();

            foreach (var item in recipe.Items) {
                if (item.Ingredient == null)
                    continue;

                // Verbrauchsmenge inkl. Schwundpuffer für dieses spezifische Rezept
                decimal amountNeededWithWaste = item.Amount * countForThisRecipe * wasteMultiplier;
                usageDict[item.Ingredient.Id] = amountNeededWithWaste;

                // Gesamtsummen für die Einkaufsliste aufsummieren
                if (ingredientAmounts.TryGetValue(item.Ingredient.Id, out (Ingredient Ingredient, decimal TotalAmount) current)) {
                    ingredientAmounts[item.Ingredient.Id] = (current.Ingredient, current.TotalAmount + item.Amount * countForThisRecipe);
                } else {
                    ingredientAmounts[item.Ingredient.Id] = (item.Ingredient, item.Amount * countForThisRecipe);
                }

                // Soll-Kosten (exakter Verbrauchswert)
                if (item.Ingredient.PackageSize > 0) {
                    decimal costPerUnit = item.Ingredient.PackagePrice / item.Ingredient.PackageSize;
                    recipeTheoreticalCost += amountNeededWithWaste * costPerUnit;
                }
            }

            recipeIngredientUsage[recipe.Id] = usageDict;

            result.RecipeCalculations.Add(new CalculatedRecipeItem {
                RecipeId = recipe.Id,
                RecipeName = recipe.Name,
                Percentage = eventRecipe.Percentage,
                TargetDrinkCount = countForThisRecipe,
                TheoreticalCostTotal = Math.Round(recipeTheoreticalCost, 2)
            });
        }

        // 2. Einkaufsliste erstellen & Gesamteinkaufskosten je Zutat ermitteln
        var ingredientTotalShoppingCost = new Dictionary<Guid, decimal>();
        var ingredientTotalAmountNeeded = new Dictionary<Guid, decimal>();

        foreach (var entry in ingredientAmounts.Values) {
            var ingredient = entry.Ingredient;
            decimal totalWithWaste = entry.TotalAmount * wasteMultiplier;

            int packagesToBuy = 0;
            decimal totalCost = 0m;

            if (ingredient.PackageSize > 0) {
                packagesToBuy = (int)Math.Ceiling(totalWithWaste / ingredient.PackageSize);
                totalCost = packagesToBuy * ingredient.PackagePrice;
            }

            ingredientTotalShoppingCost[ingredient.Id] = totalCost;
            ingredientTotalAmountNeeded[ingredient.Id] = totalWithWaste;

            result.ShoppingList.Add(new IngredientShoppingItem {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Unit = ingredient.Unit,
                TotalAmountNeeded = Math.Round(totalWithWaste, 2),
                PackageSize = ingredient.PackageSize,
                PackagePrice = ingredient.PackagePrice,
                PackagesToBuy = packagesToBuy
            });
        }

        result.TotalIceInKg = Math.Round((totalIceGrams * wasteMultiplier) / 1000m, 2);

        // 3. Realkosten verursachungsgerecht & proportional je Zutat verteilen
        foreach (var calc in result.RecipeCalculations) {
            decimal recipeRealCost = 0m;

            if (recipeIngredientUsage.TryGetValue(calc.RecipeId, out var usageDict)) {
                foreach (var (ingredientId, recipeAmount) in usageDict) {
                    decimal totalShoppingCost = ingredientTotalShoppingCost[ingredientId];
                    decimal totalAmountNeeded = ingredientTotalAmountNeeded[ingredientId];

                    if (totalAmountNeeded > 0) {
                        // Verhältnismäßiger Anteil dieses Cocktails an den Realkosten dieser Zutat
                        decimal ingredientShare = recipeAmount / totalAmountNeeded;
                        recipeRealCost += totalShoppingCost * ingredientShare;
                    }
                }
            }

            calc.RealCostTotal = Math.Round(recipeRealCost, 2);
        }

        return result;
    }
}