namespace CateringCalculator.Core.Models;

public class Recipe {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<RecipeItem> Items { get; set; } = new();
    public decimal IceInGrams { get; set; } = 150m;
    public decimal TotalCostPerDrink => Items.Sum(item => item.Cost);
}