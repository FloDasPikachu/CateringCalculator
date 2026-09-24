namespace CateringCalculator.Core.Models;

public class Recipe {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
    public List<RecipeItem> Items { get; set; } = [];

    public decimal TotalCostPerDrink => Items.Sum(item => item.Cost);
}