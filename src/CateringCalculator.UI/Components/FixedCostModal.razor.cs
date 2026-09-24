using CateringCalculator.Core.Models;
using CateringCalculator.UI.Resources.Internationalization;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class FixedCostModal {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public List<FixedCostItem> FixedItems { get; set; } = new();
    [Parameter] public Guid EventPlanId { get; set; }
    [Parameter] public EventCallback<List<FixedCostItem>> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private List<FixedCostItem> _items = new();

    protected override void OnParametersSet() {
        _items = FixedItems.Select(x => new FixedCostItem {
            Id = x.Id,
            EventPlanId = x.EventPlanId,
            Description = x.Description,
            Amount = x.Amount
        }).ToList();
    }

    private void AddFixedItem() {
        _items.Add(new FixedCostItem {
            EventPlanId = EventPlanId,
            Description = AppResources.FixedModal_DefaultDescription,
            Amount = 100m
        });
    }
}