namespace CateringCalculator.Core.Models;

public class RecipeItem {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
    public decimal AmountMl { get; set; }
    public decimal Cost => Ingredient != null ? Ingredient.PricePerMl * AmountMl : 0;
}