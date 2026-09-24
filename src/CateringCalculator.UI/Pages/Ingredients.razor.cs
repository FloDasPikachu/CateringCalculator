using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using CateringCalculator.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Pages;

public partial class Ingredients : IDisposable {
    [Inject]
    private LocalizationService LocalizationService { get; set; } = default!;

    private List<Ingredient>? _ingredients;
    private Ingredient _editingIngredient = new();
    private bool _showModal = false;
    private bool _showInfoModal = false;
    private string _infoMessage = string.Empty;

    private bool _showDeleteConfirmation = false;
    private Ingredient? _ingredientToDelete;

    protected override async Task OnInitializedAsync() {
        LocalizationService.OnChange += StateHasChanged;
        await LoadIngredientsAsync();
    }

    private async Task LoadIngredientsAsync() {
        _ingredients = await RecipeRepository.GetAllIngredientsAsync();
    }

    private void OpenAddModal() {
        _editingIngredient = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "",
            Unit = IngredientUnit.Milliliter,
            PackageSize = 1000m,
            PackagePrice = 0m
        };
        _showModal = true;
    }

    private void OpenEditModal(Ingredient ingredient) {
        _editingIngredient = new Ingredient {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Unit = ingredient.Unit,
            PackageSize = ingredient.PackageSize,
            PackagePrice = ingredient.PackagePrice
        };
        _showModal = true;
    }

    private void CloseModal() {
        _showModal = false;
    }

    private async Task HandleSaveIngredientAsync(Ingredient ingredient) {
        bool exists = _ingredients?.Any(i => i.Id == ingredient.Id) ?? false;

        if (!exists) {
            await RecipeRepository.AddIngredientAsync(ingredient);
        } else {
            await RecipeRepository.UpdateIngredientAsync(ingredient);
        }

        _showModal = false;
        await LoadIngredientsAsync();
    }


    private void ConfirmDeleteIngredient(Ingredient ingredient) {
        _ingredientToDelete = ingredient;
        _showDeleteConfirmation = true;
    }

    private void CancelDelete() {
        _ingredientToDelete = null;
        _showDeleteConfirmation = false;
    }

    private async Task ExecuteDeleteIngredientAsync() {
        if (_ingredientToDelete == null)
            return;

        try {
            await RecipeRepository.DeleteIngredientAsync(_ingredientToDelete.Id);
            _ingredientToDelete = null;
            _showDeleteConfirmation = false;
            await LoadIngredientsAsync();
        } catch (InvalidOperationException ex) {
            _showDeleteConfirmation = false;
            _ingredientToDelete = null;

            _infoMessage = ex.Message;
            _showInfoModal = true;
        }
    }

    public void Dispose() {
        LocalizationService.OnChange -= StateHasChanged;
    }

    private void CloseInfoModal() {
        _showInfoModal = false;
        _infoMessage = string.Empty;
    }
}