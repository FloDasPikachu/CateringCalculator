using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Resources.Internationalization;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CateringCalculator.UI.Pages;

public partial class Recipes : IDisposable, IAsyncDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private List<Recipe> _recipes = new();
    private List<Ingredient> _availableIngredients = new();
    private bool _isLoading = true;
    private bool _isSaving = false;
    private bool _showForm = false;
    private bool _isEditing = false;
    private string? _selectedGroupFilter;
    private bool _showDeleteConfirmation = false;
    private Recipe? _recipeToDelete;

    // Variablen für den Scroll-Observer
    private ElementReference pageHeaderRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Recipes>? _dotNetRef;

    private List<string> AllExistingGroups => _recipes?
        .SelectMany(r => r.Groups ?? new())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(g => g)
        .ToList() ?? new();

    private Recipe _editingRecipe = new();
    private string _searchTerm = string.Empty;

    protected override async Task OnInitializedAsync() {
        LocalizationService.OnChange += StateHasChanged;

        // Startet ganz oben erst einmal mit dem Standard-Titel in der Navbar
        TitleService.SetTitle("Catering Calculator");

        await LoadDataAsync();
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
            // Header ist im Sichtbereich -> Zeige Standard-App-Namen
            TitleService.SetTitle("Catering Calculator");
        } else {
            // Header wurde nach oben weggescrollt -> Zeige den Seitentitel
            TitleService.SetTitle(AppResources.Recipes_Title);
        }
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

    private void ConfirmDeleteRecipe(Recipe recipe) {
        _recipeToDelete = recipe;
        _showDeleteConfirmation = true;
    }

    private void CancelDelete() {
        _recipeToDelete = null;
        _showDeleteConfirmation = false;
    }

    private async Task ExecuteDeleteRecipeAsync() {
        if (_recipeToDelete != null) {
            await RecipeRepository.DeleteRecipeAsync(_recipeToDelete.Id);
            _recipeToDelete = null;
            _showDeleteConfirmation = false;
            await LoadDataAsync();
        }
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