using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Pages;

public partial class Recipes : IDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;

    private List<Recipe> _recipes = new();
    private List<Ingredient> _availableIngredients = new();
    private bool _isLoading = true;
    private bool _isSaving = false;
    private bool _showForm = false;
    private bool _isEditing = false;
    private string? _selectedGroupFilter;
    private bool _showDeleteConfirmation = false;
    private Recipe? _recipeToDelete;

    private List<string> AllExistingGroups => _recipes?
        .SelectMany(r => r.Groups ?? new())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(g => g)
        .ToList() ?? new();

    private Recipe _editingRecipe = new();
    private string _searchTerm = string.Empty;

    protected override async Task OnInitializedAsync() {
        LocalizationService.OnChange += StateHasChanged;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync() {
        _isLoading = true;
        _recipes = await RecipeRepository.GetAllRecipesAsync();
        _availableIngredients = await RecipeRepository.GetAllIngredientsAsync();
        _isLoading = false;
    }

    private void OpenCreateDialog() {
        _editingRecipe = new Recipe { Items = new List<RecipeItem>(), Groups = new List<string>() };
        _isEditing = false;
        _showForm = true;
    }

    private void EditRecipe(Recipe recipe) {
        _editingRecipe = new Recipe {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Groups = new List<string>(recipe.Groups ?? new()),
            Items = recipe.Items.Select(i => new RecipeItem {
                Id = i.Id,
                IngredientId = i.IngredientId,
                Ingredient = i.Ingredient,
                Amount = i.Amount
            }).ToList()
        };
        _isEditing = true;
        _showForm = true;
    }

    private void CloseForm() {
        _showForm = false;
        _isEditing = false;
    }

    private async Task HandleIngredientAddedAsync(Ingredient newIngredient) {
        await RecipeRepository.AddIngredientAsync(newIngredient);
        _availableIngredients = await RecipeRepository.GetAllIngredientsAsync();
    }

    private async Task SaveRecipeAsync(Recipe recipeToSave) {
        if (_isSaving)
            return;
        _isSaving = true;

        try {
            if (!_isEditing || recipeToSave.Id == Guid.Empty) {
                await RecipeRepository.AddRecipeAsync(recipeToSave);
            } else {
                await RecipeRepository.UpdateRecipeAsync(recipeToSave);
            }

            _recipes = await RecipeRepository.GetAllRecipesAsync();
            CloseForm();
        } finally {
            _isSaving = false;
        }
    }

    private IEnumerable<(Recipe Recipe, int Score)> EvaluatedRecipes {
        get {
            if (_recipes == null)
                yield break;

            var searchTerms = ParseSearchTerms(_searchTerm);
            bool hasGroupFilter = !string.IsNullOrWhiteSpace(_selectedGroupFilter);
            bool hasAnyFilter = searchTerms.Length > 0 || hasGroupFilter;

            foreach (var recipe in _recipes) {
                int score = CalculateRecipeScore(recipe, searchTerms, hasGroupFilter);
                yield return (recipe, hasAnyFilter ? score : 0);
            }
        }
    }

    private static string[] ParseSearchTerms(string searchTerm) {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<string>();

        return searchTerm.ToLowerInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();
    }

    private int CalculateRecipeScore(Recipe recipe, string[] searchTerms, bool hasGroupFilter) {
        // 1. Gruppen-Prüfung
        if (hasGroupFilter && !MatchesGroupFilter(recipe))
            return 0;

        // 2. Wenn kein Suchbegriff aktiv ist, aber die Gruppe passt -> Basis-Score (Top-Treffer)
        if (searchTerms.Length == 0)
            return hasGroupFilter ? 10 : 0;

        // 3. Suchbegriffe auswerten
        int searchScore = CalculateSearchScore(recipe, searchTerms);
        return searchScore > 0 ? 10 + searchScore : 0;
    }

    private bool MatchesGroupFilter(Recipe recipe) {
        return recipe.Groups != null &&
               recipe.Groups.Contains(_selectedGroupFilter, StringComparer.OrdinalIgnoreCase);
    }

    private static int CalculateSearchScore(Recipe recipe, string[] searchTerms) {
        string name = recipe.Name.ToLowerInvariant();
        string desc = recipe.Description?.ToLowerInvariant() ?? string.Empty;
        string groups = recipe.Groups != null ? string.Join(" ", recipe.Groups).ToLowerInvariant() : string.Empty;
        string ingredients = string.Join(" ", recipe.Items.Select(i => i.Ingredient?.Name ?? string.Empty)).ToLowerInvariant();

        int totalTermScore = 0;
        bool anyTermMatched = false;

        foreach (var term in searchTerms) {
            int termScore = 0;
            if (name.Contains(term))
                termScore += 10;
            if (groups.Contains(term))
                termScore += 8;
            if (ingredients.Contains(term))
                termScore += 5;
            if (desc.Contains(term))
                termScore += 2;

            if (termScore > 0) {
                anyTermMatched = true;
                totalTermScore += termScore;
            }
        }

        return anyTermMatched ? totalTermScore : 0;
    }

    private void FilterByGroup(string group) {
        _selectedGroupFilter = group;
        StateHasChanged();
    }

    private void ClearGroupFilter() {
        _selectedGroupFilter = null;
        StateHasChanged();
    }

    private void ClearSearch() {
        _searchTerm = string.Empty;
        StateHasChanged();
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
    }

    private void ConfirmDeleteRecipe(Recipe recipe) {
        _recipeToDelete = recipe;
        _showDeleteConfirmation = true;
    }

    private void CancelDelete() {
        _recipeToDelete = null;
        _showDeleteConfirmation = false;
    }

    // Führt das eigentliche Löschen nach Bestätigung aus
    private async Task ExecuteDeleteRecipeAsync() {
        if (_recipeToDelete != null) {
            await RecipeRepository.DeleteRecipeAsync(_recipeToDelete.Id);
            _recipeToDelete = null;
            _showDeleteConfirmation = false;
            await LoadDataAsync();
        }
    }
}