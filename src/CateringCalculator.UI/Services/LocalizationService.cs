using System.Globalization;

namespace CateringCalculator.UI.Services;

public class LocalizationService {
    private CultureInfo _currentCulture = new("de");

    public CultureInfo CurrentCulture {
        get => _currentCulture;
        set {
            if (_currentCulture != value) {
                _currentCulture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
                OnChange?.Invoke();
            }
        }
    }

    public event Action? OnChange;

    public void SetCulture(string cultureCode) {
        CurrentCulture = new CultureInfo(cultureCode);
    }
}