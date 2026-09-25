using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable {
    [Inject] private LocalizationService LocalizationService { get; set; } = default!;

    private bool _isDesktopMenuOpen = false;
    private bool IsMobileDevice => DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
    private bool IsGermanActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "de";
    private bool IsEnglishActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "en";

    protected override void OnInitialized() {
        LocalizationService.OnChange += StateHasChanged;
    }


    private void ChangeLanguage(string cultureCode) {
        LocalizationService.SetCulture(cultureCode);
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
    }

    private void ToggleDesktopMenu() => _isDesktopMenuOpen = !_isDesktopMenuOpen;
    private void CloseDesktopMenu() => _isDesktopMenuOpen = false;
}