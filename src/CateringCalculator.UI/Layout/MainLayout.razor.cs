using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Devices;
using CateringCalculator.UI.Shared;
using CateringCalculator.UI.Services;

namespace CateringCalculator.UI.Layout;

public partial class MainLayout : LocalizedLayoutBase {
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;

    private bool _isDesktopMenuOpen = false;
    private bool _isDarkMode = true;

    private bool IsMobileDevice => DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

    private bool IsGermanActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "de";
    private bool IsEnglishActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "en";

    protected override void OnInitialized() {
        base.OnInitialized();
        TitleService.OnChange += StateHasChanged;
    }

    private void ChangeLanguage(string cultureCode) {
        LocalizationService.SetCulture(cultureCode);
    }

    private void ToggleDesktopMenu() {
        _isDesktopMenuOpen = !_isDesktopMenuOpen;
    }

    private void CloseDesktopMenu() {
        _isDesktopMenuOpen = false;
    }

    private void ToggleTheme() {
        _isDarkMode = !_isDarkMode;
        StateHasChanged();
    }

    public override void Dispose() {
        base.Dispose();
        TitleService.OnChange -= StateHasChanged;
    }
}
