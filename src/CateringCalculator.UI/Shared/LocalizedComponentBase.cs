using System;
using Microsoft.AspNetCore.Components;
using CateringCalculator.UI.Services;

namespace CateringCalculator.UI.Shared;

public abstract class LocalizedComponentBase : ComponentBase, IDisposable {
    [Inject]
    protected LocalizationService LocalizationService { get; set; } = default!;

    protected override void OnInitialized() {
        base.OnInitialized();
        LocalizationService.OnChange += OnLanguageChanged;
    }

    protected virtual void OnLanguageChanged() {
        InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose() {
        LocalizationService.OnChange -= OnLanguageChanged;
    }
}
