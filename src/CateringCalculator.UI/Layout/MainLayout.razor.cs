using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Devices;

namespace CateringCalculator.UI.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;

    private bool _isDesktopMenuOpen = false;
    private bool _isDarkMode = true;

    private bool IsMobileDevice => DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

    private bool IsGermanActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "de";
    private bool IsEnglishActive => LocalizationService.CurrentCulture.TwoLetterISOLanguageName == "en";

    protected override void OnInitialized() {
        LocalizationService.OnChange += StateHasChanged;
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

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
        TitleService.OnChange -= StateHasChanged;
    }
}