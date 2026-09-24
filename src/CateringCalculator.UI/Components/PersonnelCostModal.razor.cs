using CateringCalculator.Core.Models;
using CateringCalculator.UI.Resources.Internationalization;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class PersonnelCostModal {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public List<PersonnelCostItem> PersonnelItems { get; set; } = new();
    [Parameter] public Guid EventPlanId { get; set; }
    [Parameter] public EventCallback<List<PersonnelCostItem>> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private List<PersonnelCostItem> _items = new();

    protected override void OnParametersSet() {
        // Lokale Kopie erzeugen, damit Änderungen beim Abbrechen verworfen werden
        _items = PersonnelItems.Select(x => new PersonnelCostItem {
            Id = x.Id,
            EventPlanId = x.EventPlanId,
            RoleName = x.RoleName,
            Count = x.Count,
            Hours = x.Hours,
            HourlyRate = x.HourlyRate
        }).ToList();
    }

    private void AddPersonnelItem() {
        _items.Add(new PersonnelCostItem {
            EventPlanId = EventPlanId,
            RoleName = AppResources.PersonnelModal_DefaultRole,
            Count = 1,
            Hours = 5m,
            HourlyRate = 14m
        });
    }
}