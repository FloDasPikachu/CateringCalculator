using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Core.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Pages;

public partial class EventPlanner {
    private bool _showPersonnelModal = false;
    private bool _showFixedModal = false;
    private EventPlan _eventPlan = CreateEmptyEventPlan();
    private List<EventPlan> _savedEventPlans = new();
    private List<Recipe> _availableRecipes = new();
    private CalculationResult? _calculationResult;

    private decimal TotalPercentage => _eventPlan.SelectedRecipes.Sum(r => r.Percentage);

    protected override async Task OnInitializedAsync() {
        _availableRecipes = await RecipeRepository.GetAllRecipesAsync();
        await LoadSavedEventPlansAsync();
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
}