namespace CateringCalculator.Core.Models;

public class Ingredient {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal BottleVolumeMl { get; set; }
    public decimal BottlePrice { get; set; }
    public decimal PricePerMl => BottleVolumeMl > 0 ? BottlePrice / BottleVolumeMl : 0;
}