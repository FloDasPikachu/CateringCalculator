using CateringCalculator.Core.Enums;

namespace CateringCalculator.Core.Models;

public class Ingredient {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; } = IngredientUnit.Milliliter;
    public decimal PackageSize { get; set; }
    public decimal PackagePrice { get; set; }
    public decimal PricePerUnit => PackageSize > 0 ? PackagePrice / PackageSize : 0;
}