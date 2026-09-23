namespace CateringCalculator.Core.Models;

public class CalculationResult {
    public Guid EventPlanId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public int TotalDrinksCount { get; set; }
    public int FreeDrinksCount { get; set; }
    public int TotalProductionDrinks { get; set; }

    public decimal FixedCosts { get; set; }
    public decimal PersonnelCosts { get; set; }
    public decimal TargetProfit { get; set; }

    public List<IngredientShoppingItem> ShoppingList { get; set; } = new();
    public List<CalculatedRecipeItem> RecipeCalculations { get; set; } = new();

    public decimal TotalMaterialCost => ShoppingList.Sum(x => x.TotalCost);
    public decimal TotalEventCosts => TotalMaterialCost + FixedCosts + PersonnelCosts;
    public decimal TotalTargetRevenue => TotalEventCosts + TargetProfit;

    public decimal CostPerDrink => TotalDrinksCount > 0 ? TotalMaterialCost / TotalDrinksCount : 0m;
    public decimal TargetSalesPricePerDrink => TotalDrinksCount > 0 ? TotalTargetRevenue / TotalDrinksCount : 0m;
}