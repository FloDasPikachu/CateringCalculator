namespace CateringCalculator.Core.Models;

public class CalculationResult {
    public Guid EventPlanId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int TotalDrinksCount { get; set; }
    public List<IngredientShoppingItem> ShoppingList { get; set; } = new();
    public List<CalculatedRecipeItem> RecipeCalculations { get; set; } = new();
    public decimal TotalMaterialCost => ShoppingList.Sum(x => x.TotalCost);
    public decimal CostPerDrink => TotalDrinksCount > 0 ? TotalMaterialCost / TotalDrinksCount : 0;
}