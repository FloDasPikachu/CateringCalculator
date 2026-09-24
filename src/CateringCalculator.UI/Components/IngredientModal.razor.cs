using CateringCalculator.Core.Models;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class IngredientModal {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Ingredient Ingredient { get; set; } = new();
    [Parameter] public EventCallback<Ingredient> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task HandleValidSubmit() {
        await OnSave.InvokeAsync(Ingredient);
    }
}