namespace CateringCalculator.Core.Models;

public class IngredientShoppingItem {
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal TotalVolumeNeededMl { get; set; }
    public decimal BottleVolumeMl { get; set; }
    public int BottlesToBuy { get; set; }
    public decimal BottlePrice { get; set; }
    public decimal TotalCost => BottlesToBuy * BottlePrice;
}