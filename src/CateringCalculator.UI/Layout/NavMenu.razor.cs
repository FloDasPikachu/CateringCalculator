using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Layout;

public partial class NavMenu : ComponentBase, IDisposable {
    [Inject] private LocalizationService LocalizationService { get; set; } = default!;

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