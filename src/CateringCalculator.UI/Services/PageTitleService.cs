namespace CateringCalculator.UI.Services;

public class PageTitleService {
    public string CurrentTitle { get; private set; } = "Catering Calculator";

    public event Action? OnChange;

    public void SetTitle(string title) {
        CurrentTitle = title;
        OnChange?.Invoke();
    }
}