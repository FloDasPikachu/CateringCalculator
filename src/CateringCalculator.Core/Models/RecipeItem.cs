namespace CateringCalculator.Core.Models;

public class RecipeItem {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
    public decimal Amount { get; set; }
    public decimal Cost => Ingredient != null ? Ingredient.PricePerUnit * Amount : 0;
}