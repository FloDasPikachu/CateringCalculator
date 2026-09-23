namespace CateringCalculator.Core.Models;

public class EventRecipe {
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventPlanId { get; set; }

    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public decimal Percentage { get; set; }
}