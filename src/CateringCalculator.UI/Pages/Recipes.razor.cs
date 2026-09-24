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

    private Recipe _editingRecipe = new();

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
        _editingRecipe = new Recipe { Items = new List<RecipeItem>() };
        _isEditing = false;
        _showForm = true;
    }

    private void EditRecipe(Recipe recipe) {
        _editingRecipe = new Recipe {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
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
}