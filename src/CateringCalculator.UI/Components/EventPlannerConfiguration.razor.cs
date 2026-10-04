using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class EventPlannerConfiguration {
    [Inject]
    private IRecipeRepository RecipeRepository { get; set; } = default!;

    [Parameter]
    public EventCallback CalculateEvent { get; set; }
    [Parameter]
    public EventPlan EventPlan { get; set; }

    private List<Recipe> _availableRecipes = [];
    private bool _showPersonnelModal = false;
    private bool _showFixedModal = false;
    private decimal TotalPercentage => EventPlan.SelectedRecipes.Sum(r => r.Percentage);

    protected override async Task OnInitializedAsync() {
        _availableRecipes = await RecipeRepository.GetAllRecipesAsync();
    }

    private void CloseFixedModal() => _showFixedModal = false;

    private void ClosePersonnelModal() => _showPersonnelModal = false;

    private void DistributeEvenly() {
        int count = EventPlan.SelectedRecipes.Count;
        if (count == 0)
            return;

        decimal evenShare = Math.Floor(100m / count);
        decimal remainder = 100m - (evenShare * count);

        for (int i = 0; i < count; i++) {
            EventPlan.SelectedRecipes[i].Percentage = evenShare + (i == 0 ? remainder : 0m);
        }
    }

    private IEnumerable<Ingredient> GetUniqueIngredientsForSelectedRecipes() {
        return EventPlan.SelectedRecipes
            .Where(er => er.Recipe?.Items != null)
            .SelectMany(er => er.Recipe!.Items)
            .Where(item => item.Ingredient != null)
            .Select(item => item.Ingredient!)
            .DistinctBy(i => i.Id)
            .OrderBy(i => i.Name);
    }

    private void OpenFixedModal() => _showFixedModal = true;

    private void OpenPersonnelModal() => _showPersonnelModal = true;

    private void SaveFixedModal(List<FixedCostItem> items) {
        EventPlan.FixedCostItems = items;
        EventPlan.FixedCosts = items.Sum(x => x.Amount);
        _showFixedModal = false;
    }

    private void SavePersonnelModal(List<PersonnelCostItem> items) {
        EventPlan.PersonnelCostItems = items;
        EventPlan.PersonnelCosts = items.Sum(x => x.TotalCost);
        _showPersonnelModal = false;
    }

    private void ToggleRecipeSelection(Recipe recipe) {
        var existing = EventPlan.SelectedRecipes.FirstOrDefault(r => r.RecipeId == recipe.Id);
        if (existing != null) {
            EventPlan.SelectedRecipes.Remove(existing);
        } else {
            EventPlan.SelectedRecipes.Add(new EventRecipe {
                EventPlanId = EventPlan.Id,
                RecipeId = recipe.Id,
                Recipe = recipe,
                Percentage = 0m
            });
            DistributeEvenly();
        }
    }
}
