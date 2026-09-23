namespace CateringCalculator.Core.Models;

public class PersonnelCostItem {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventPlanId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public decimal Hours { get; set; } = 6m;
    public decimal HourlyRate { get; set; } = 15m;

    public decimal TotalCost => Count * Hours * HourlyRate;
}