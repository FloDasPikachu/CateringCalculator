namespace CateringCalculator.Core.Models;

public class CalculatedRecipeItem {
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int TargetDrinkCount { get; set; }

    public decimal TheoreticalCostTotal { get; set; }
    public decimal TheoreticalCostPerDrink => TargetDrinkCount > 0 ? TheoreticalCostTotal / TargetDrinkCount : 0m;

    public decimal RealCostTotal { get; set; }
    public decimal RealCostPerDrink => TargetDrinkCount > 0 ? RealCostTotal / TargetDrinkCount : 0m;

    public decimal TargetSalesPrice { get; set; }
}