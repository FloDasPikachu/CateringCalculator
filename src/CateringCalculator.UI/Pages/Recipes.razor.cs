using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Pages;

public partial class Recipes : IDisposable {
    [Inject] private LocalizationService LocalizationService { get; set; } = default!;

    private List<Recipe> _recipes = new();
    private List<Ingredient> _availableIngredients = new();
    private bool _isLoading = true;
    private bool _isSaving = false;
    private bool _showForm = false;
    private bool _isEditing = false;
    private string? _selectedGroupFilter;
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

    private async Task DeleteRecipeAsync(Guid recipeId) {
        await RecipeRepository.DeleteRecipeAsync(recipeId);
        await LoadDataAsync();
    }

    // Live-Suche & Gruppenfilter-Evaluierung (unterstützt kommagetrennte Suche)
    private IEnumerable<(Recipe Recipe, int Score)> EvaluatedRecipes {
        get {
            if (_recipes == null)
                yield break;

            bool hasSearch = !string.IsNullOrWhiteSpace(_searchTerm);
            bool hasGroup = !string.IsNullOrWhiteSpace(_selectedGroupFilter);
            bool anyFilterActive = hasSearch || hasGroup;

            // Mehrere Begriffe anhand von Kommas trennen (und Leerzeichen außenrum entfernen)
            var searchTerms = hasSearch
                ? _searchTerm.ToLowerInvariant()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToArray()
                : Array.Empty<string>();

            bool effectiveHasSearch = searchTerms.Length > 0;
            bool effectiveAnyFilterActive = effectiveHasSearch || hasGroup;

            foreach (var recipe in _recipes) {
                int score = 0;

                // 1. Prüfen, ob die Gruppe übereinstimmt (falls Gruppenfilter aktiv)
                bool matchesGroup = !hasGroup || (recipe.Groups != null && recipe.Groups.Contains(_selectedGroupFilter, StringComparer.OrdinalIgnoreCase));

                // 2. Prüfen, ob mind. einer der kommagetrennten Suchbegriffe matcht
                bool matchesSearch = true;
                int searchScore = 0;

                if (effectiveHasSearch) {
                    string name = recipe.Name.ToLowerInvariant();
                    string desc = recipe.Description.ToLowerInvariant();
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

                    matchesSearch = anyTermMatched;
                    searchScore = totalTermScore;
                }

                // Ein Cocktail ist ein Top-Treffer, wenn alle aktiven Filter matchen
                if (effectiveAnyFilterActive) {
                    bool groupConditionMet = !hasGroup || matchesGroup;
                    bool searchConditionMet = !effectiveHasSearch || matchesSearch;

                    // Wenn beide (bzw. die jeweils aktiven) Bedingungen erfüllt sind -> Top-Treffer
                    if (groupConditionMet && searchConditionMet) {
                        score = 10 + searchScore;
                    } else {
                        score = 0; // Andernfalls in den Rest ("Weitere Cocktails")
                    }
                } else {
                    score = 0;
                }

                yield return (recipe, score);
            }
        }
    }

    private static string GetUnitSuffix(IngredientUnit unit) => unit switch {
        IngredientUnit.Piece => "Stk.",
        IngredientUnit.Gram => "g",
        IngredientUnit.Milliliter => "ml",
        _ => "ml"
    };

    private static string FormatAmount(decimal amount, IngredientUnit? unit) {
        var suffix = unit.HasValue ? GetUnitSuffix(unit.Value) : "ml";
        return $"{amount:0.##} {suffix}";
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
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
}