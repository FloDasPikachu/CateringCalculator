using CateringCalculator.Core.Models;

namespace CateringCalculator.Core.Services;

public class CalculationService {
    public CalculationResult Calculate(EventPlan eventPlan) {
        ArgumentNullException.ThrowIfNull(eventPlan);

        var context = new CalculationContext(eventPlan);

        if (!context.IsValid) {
            return context.Result;
        }

        CalculateRecipeRequirements(eventPlan, context);
        GenerateShoppingList(context);
        DistributeRealCosts(context);
        CalculatePricing(eventPlan, context);

        context.Result.RecipeCalculations = [.. context.Result.RecipeCalculations.OrderBy(r => r.RecipeName)];
        context.Result.ShoppingList = [.. context.Result.ShoppingList.OrderBy(s => s.IngredientName)];

        return context.Result;
    }

    private static void AccumulateIngredientAmounts(CalculationContext context, Ingredient ingredient, decimal amount) {
        if (context.IngredientAmounts.TryGetValue(ingredient.Id, out var current)) {
            context.IngredientAmounts[ingredient.Id] = (current.Ingredient, current.TotalAmount + amount);
        } else {
            context.IngredientAmounts[ingredient.Id] = (ingredient, amount);
        }
    }

    private static void CalculatePricing(EventPlan eventPlan, CalculationContext context) {
        decimal totalProductionMaterialCost = context.Result.RecipeCalculations.Sum(c => c.RealCostTotal);

        decimal generalMarkupPerPayingDrink = 0m;
        if (context.PayingDrinksCount > 0) {
            decimal materialCostOfFreeDrinks = context.TotalProductionDrinks > 0
                ? totalProductionMaterialCost * ((decimal)context.FreeDrinksCount / context.TotalProductionDrinks)
                : 0m;

            generalMarkupPerPayingDrink = (eventPlan.FixedCosts + eventPlan.PersonnelCosts + eventPlan.TargetProfit + materialCostOfFreeDrinks) / context.PayingDrinksCount;
        }

        foreach (var calc in context.Result.RecipeCalculations) {
            decimal realCostPerProducedDrink = calc.TargetDrinkCount > 0 ? calc.RealCostTotal / calc.TargetDrinkCount : 0m;
            calc.TargetSalesPrice = Math.Round(realCostPerProducedDrink + generalMarkupPerPayingDrink, 2);
        }
    }

    private static void CalculateRecipeRequirements(EventPlan eventPlan, CalculationContext context) {
        foreach (var eventRecipe in eventPlan.SelectedRecipes) {
            var recipe = eventRecipe.Recipe;
            if (recipe == null)
                continue;

            int payingForThisRecipe = (int)Math.Round(context.PayingDrinksCount * (eventRecipe.Percentage / 100m));
            int freeForThisRecipe = (int)Math.Round(context.FreeDrinksCount * (eventRecipe.Percentage / 100m));
            int countForThisRecipe = payingForThisRecipe + freeForThisRecipe;

            decimal recipeTheoreticalCost = 0m;
            var usageDict = new Dictionary<Guid, decimal>();

            foreach (var item in recipe.Items) {
                if (item.Ingredient == null)
                    continue;

                decimal amountNeededWithWaste = item.Amount * countForThisRecipe * context.WasteMultiplier;
                if (usageDict.TryGetValue(item.Ingredient.Id, out decimal existingAmount)) {
                    usageDict[item.Ingredient.Id] = existingAmount + amountNeededWithWaste;
                } else {
                    usageDict[item.Ingredient.Id] = amountNeededWithWaste;
                }

                AccumulateIngredientAmounts(context, item.Ingredient, item.Amount * countForThisRecipe);

                if (item.Ingredient.PackageSize > 0) {
                    decimal costPerUnit = item.Ingredient.PackagePrice / item.Ingredient.PackageSize;
                    recipeTheoreticalCost += amountNeededWithWaste * costPerUnit;
                }
            }

            context.RecipeIngredientUsage[recipe.Id] = usageDict;

            context.Result.RecipeCalculations.Add(new CalculatedRecipeItem {
                RecipeId = recipe.Id,
                RecipeName = recipe.Name,
                Percentage = eventRecipe.Percentage,
                PayingDrinkCount = payingForThisRecipe,
                FreeDrinkCount = freeForThisRecipe,
                TheoreticalCostTotal = Math.Round(recipeTheoreticalCost, 2)
            });
        }
    }

    private static void DistributeRealCosts(CalculationContext context) {
        foreach (var calc in context.Result.RecipeCalculations) {
            decimal recipeRealCost = 0m;

            if (context.RecipeIngredientUsage.TryGetValue(calc.RecipeId, out var usageDict)) {
                foreach (var (ingredientId, recipeAmount) in usageDict) {
                    decimal totalShoppingCost = context.IngredientTotalShoppingCost[ingredientId];
                    decimal totalAmountNeeded = context.IngredientTotalAmountNeeded[ingredientId];

                    if (totalAmountNeeded > 0) {
                        decimal ingredientShare = recipeAmount / totalAmountNeeded;
                        recipeRealCost += totalShoppingCost * ingredientShare;
                    }
                }
            }

            calc.RealCostTotal = Math.Round(recipeRealCost, 2);
        }
    }

    private static void GenerateShoppingList(CalculationContext context) {
        foreach (var entry in context.IngredientAmounts.Values) {
            var ingredient = entry.Ingredient;
            decimal totalWithWaste = entry.TotalAmount * context.WasteMultiplier;
            var useStock = context.UseStock && context.IngredientsUseStock.Contains(ingredient.Id);
            decimal effectiveStock = useStock ? ingredient.StockAmount : 0m;
            decimal amountToBuyNet = Math.Max(0m, totalWithWaste - effectiveStock);
            int packagesToBuy = 0;

            if (ingredient.PackageSize > 0 && amountToBuyNet > 0) {
                packagesToBuy = (int)Math.Ceiling(amountToBuyNet / ingredient.PackageSize);
            }

            decimal totalCost = packagesToBuy * ingredient.PackagePrice;
            context.IngredientTotalShoppingCost[ingredient.Id] = totalCost;
            context.IngredientTotalAmountNeeded[ingredient.Id] = totalWithWaste;
            context.Result.ShoppingList.Add(new IngredientShoppingItem {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Unit = ingredient.Unit,
                TotalAmountNeeded = Math.Round(totalWithWaste, 2),
                StockAmount = ingredient.StockAmount,
                AmountToBuyNet = Math.Round(amountToBuyNet, 2),
                PackageSize = ingredient.PackageSize,
                PackagePrice = ingredient.PackagePrice,
                PackagesToBuy = packagesToBuy,
                IsStockUsed = useStock
            });
        }
    }

    private sealed class CalculationContext {
        public CalculationResult Result { get; }
        public int PayingDrinksCount { get; }
        public int FreeDrinksCount { get; }
        public int TotalProductionDrinks { get; }
        public decimal WasteMultiplier { get; }
        public bool UseStock { get; }

        public bool IsValid => PayingDrinksCount > 0 && _hasRecipes;

        private readonly bool _hasRecipes;

        public Dictionary<Guid, (Ingredient Ingredient, decimal TotalAmount)> IngredientAmounts { get; } = new();
        public Dictionary<Guid, Dictionary<Guid, decimal>> RecipeIngredientUsage { get; } = new();
        public Dictionary<Guid, decimal> IngredientTotalShoppingCost { get; } = new();
        public Dictionary<Guid, decimal> IngredientTotalAmountNeeded { get; } = new();
        public HashSet<Guid> IngredientsUseStock { get; } = new();

        public CalculationContext(EventPlan eventPlan) {
            PayingDrinksCount = eventPlan.TotalDrinksToServe;
            FreeDrinksCount = eventPlan.FreeDrinksCount;
            TotalProductionDrinks = PayingDrinksCount + FreeDrinksCount;
            WasteMultiplier = 1m + (eventPlan.WasteBufferPercent / 100m);
            _hasRecipes = eventPlan.SelectedRecipes.Count > 0;
            UseStock = eventPlan.UseStock;

            Result = new CalculationResult {
                EventPlanId = eventPlan.Id,
                EventTitle = eventPlan.Title,
                TotalDrinksCount = PayingDrinksCount,
                FreeDrinksCount = FreeDrinksCount,
                FixedCosts = eventPlan.FixedCosts,
                PersonnelCosts = eventPlan.PersonnelCosts,
                TargetProfit = eventPlan.TargetProfit
            };

            if (UseStock && eventPlan.IngredientsUseStock != null) {
                foreach (var id in eventPlan.IngredientsUseStock) {
                    IngredientsUseStock.Add(id);
                }
            }
        }
    }
}