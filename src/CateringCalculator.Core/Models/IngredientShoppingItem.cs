using CateringCalculator.Core.Enums;

namespace CateringCalculator.Core.Models;

public class IngredientShoppingItem {
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; } = IngredientUnit.Milliliter;
    public decimal TotalAmountNeeded { get; set; }
    public decimal PackageSize { get; set; }
    public decimal PackagePrice { get; set; }
    public int PackagesToBuy { get; set; }
    public decimal TotalCost => PackagesToBuy * PackagePrice;
}