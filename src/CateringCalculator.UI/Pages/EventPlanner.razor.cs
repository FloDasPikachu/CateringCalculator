using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Core.Services;
using CateringCalculator.UI.Resources.Internationalization;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CateringCalculator.UI.Pages;

public partial class EventPlanner : IDisposable, IAsyncDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private bool _showPersonnelModal = false;
    private bool _showFixedModal = false;
    private EventPlan _eventPlan = CreateEmptyEventPlan();
    private List<EventPlan> _savedEventPlans = new();
    private List<Recipe> _availableRecipes = new();
    private CalculationResult? _calculationResult;

    // Variablen für den Scroll-Observer
    private ElementReference pageHeaderRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<EventPlanner>? _dotNetRef;

    private decimal TotalPercentage => _eventPlan.SelectedRecipes.Sum(r => r.Percentage);

    protected override async Task OnInitializedAsync() {
        LocalizationService.OnChange += StateHasChanged;

        // Startet ganz oben mit dem Standard-Titel
        TitleService.SetTitle("Catering Calculator");

        _availableRecipes = await RecipeRepository.GetAllRecipesAsync();
        await LoadSavedEventPlansAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            _dotNetRef = DotNetObjectReference.Create(this);
            // Lädt das JS-Skript aus wwwroot/js/scrollObserver.js
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/scrollObserver.js");
            await _jsModule.InvokeVoidAsync("observeHeader", pageHeaderRef, _dotNetRef);
        }
    }

    [JSInvokable]
    public void TargetVisibilityChanged(bool isIntersecting) {
        if (isIntersecting) {
            // Header im Sichtbereich -> Standard-App-Namen anzeigen
            TitleService.SetTitle("Catering Calculator");
        } else {
            // Header nach oben weggescrollt -> Seitentitel anzeigen
            TitleService.SetTitle(AppResources.Event_Title);
        }
    }

    private async Task LoadSavedEventPlansAsync() {
        _savedEventPlans = await EventPlanRepository.GetAllEventPlansAsync();
    }

    private static EventPlan CreateEmptyEventPlan() => new() {
        Id = Guid.NewGuid(),
        Title = "Neues Event",
        GuestCount = 30,
        AverageDrinksPerGuest = 3,
        WasteBufferPercent = 10m,
        FixedCosts = 150m,
        PersonnelCosts = 300m,
        TargetProfit = 500m,
        FreeDrinksCount = 10,
        SelectedRecipes = new List<EventRecipe>()
    };

    private void CreateNewEvent() {
        _eventPlan = CreateEmptyEventPlan();
        _calculationResult = null;
    }

    private async Task OnEventSelected(ChangeEventArgs e) {
        if (Guid.TryParse(e.Value?.ToString(), out Guid id) && id != Guid.Empty) {
            var loaded = await EventPlanRepository.GetEventPlanByIdAsync(id);
            if (loaded != null) {
                _eventPlan = loaded;
                _calculationResult = null;
            }
        } else {
            CreateNewEvent();
        }
    }

    private async Task SaveCurrentEvent() {
        await EventPlanRepository.SaveEventPlanAsync(_eventPlan);
        await LoadSavedEventPlansAsync();
    }

    private async Task DeleteCurrentEvent() {
        if (_eventPlan.Id != Guid.Empty) {
            await EventPlanRepository.DeleteEventPlanAsync(_eventPlan.Id);
            await LoadSavedEventPlansAsync();
            CreateNewEvent();
        }
    }

    private void ToggleRecipeSelection(Recipe recipe) {
        var existing = _eventPlan.SelectedRecipes.FirstOrDefault(r => r.RecipeId == recipe.Id);
        if (existing != null) {
            _eventPlan.SelectedRecipes.Remove(existing);
        } else {
            _eventPlan.SelectedRecipes.Add(new EventRecipe {
                EventPlanId = _eventPlan.Id,
                RecipeId = recipe.Id,
                Recipe = recipe,
                Percentage = 0m
            });
            DistributeEvenly();
        }
    }

    private void DistributeEvenly() {
        int count = _eventPlan.SelectedRecipes.Count;
        if (count == 0)
            return;

        decimal evenShare = Math.Floor(100m / count);
        decimal remainder = 100m - (evenShare * count);

        for (int i = 0; i < count; i++) {
            _eventPlan.SelectedRecipes[i].Percentage = evenShare + (i == 0 ? remainder : 0m);
        }
    }

    private void CalculateEvent() {
        _calculationResult = CalculationService.Calculate(_eventPlan);
    }

    private async Task ExportPdf() {
        if (_calculationResult == null)
            return;

        var pdfBytes = CateringCalculator.Infrastructure.Services.ShoppingListPdfGenerator.GeneratePdf(_calculationResult);
        var fileName = $"Einkaufsliste_{_calculationResult.EventTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

        await FileSaveService.SaveAndOpenFileAsync(fileName, pdfBytes);
    }

    private static string GetUnitSuffix(IngredientUnit unit) => unit switch {
        IngredientUnit.Piece => "Stk.",
        IngredientUnit.Gram => "g",
        IngredientUnit.Milliliter => "ml",
        _ => "ml"
    };

    // -- Personal Modal Steuerung --
    private void OpenPersonnelModal() => _showPersonnelModal = true;
    private void ClosePersonnelModal() => _showPersonnelModal = false;
    private void SavePersonnelModal(List<PersonnelCostItem> items) {
        _eventPlan.PersonnelCostItems = items;
        _eventPlan.PersonnelCosts = items.Sum(x => x.TotalCost);
        _showPersonnelModal = false;
    }

    // -- Fixkosten Modal Steuerung --
    private void OpenFixedModal() => _showFixedModal = true;
    private void CloseFixedModal() => _showFixedModal = false;
    private void SaveFixedModal(List<FixedCostItem> items) {
        _eventPlan.FixedCostItems = items;
        _eventPlan.FixedCosts = items.Sum(x => x.Amount);
        _showFixedModal = false;
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
    }

    public async ValueTask DisposeAsync() {
        if (_jsModule != null) {
            await _jsModule.InvokeVoidAsync("unobserveHeader");
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}