using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Layout;

public partial class NavMenu : ComponentBase, IDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;

    [Parameter]
    public bool IsMobile { get; set; }
    [Parameter]
    public EventCallback OnItemSelected { get; set; }

    private async Task HandleClick() {
        if (OnItemSelected.HasDelegate) {
            await OnItemSelected.InvokeAsync();
        }
    }

    protected override void OnInitialized() {
        LocalizationService.OnChange += OnLanguageChanged;
    }

    private void OnLanguageChanged() {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() {
        LocalizationService.OnChange -= OnLanguageChanged;
    }
}