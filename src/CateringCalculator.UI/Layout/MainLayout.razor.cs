using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable {
    [Inject] private LocalizationService LocalizationService { get; set; } = default!;

    protected override void OnInitialized() {
        LocalizationService.OnChange += StateHasChanged;
    }

    private bool IsGermanActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "de";
    private bool IsEnglishActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "en";

    private void ChangeLanguage(string cultureCode) {
        LocalizationService.SetCulture(cultureCode);
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
    }
}