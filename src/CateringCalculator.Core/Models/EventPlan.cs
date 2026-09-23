namespace CateringCalculator.Core.Models;

public class EventPlan {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public int AverageDrinksPerGuest { get; set; } = 3;
    public decimal WasteBufferPercent { get; set; } = 10m;

    public decimal FixedCosts { get; set; } = 0m;
    public decimal PersonnelCosts { get; set; } = 0m;
    public decimal TargetProfit { get; set; } = 0m;
    public int FreeDrinksCount { get; set; } = 0;

    public List<EventRecipe> SelectedRecipes { get; set; } = [];
    public int TotalDrinksToServe => GuestCount * AverageDrinksPerGuest;
}