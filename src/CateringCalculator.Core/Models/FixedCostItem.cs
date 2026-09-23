namespace CateringCalculator.Core.Models;

public class FixedCostItem {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventPlanId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}