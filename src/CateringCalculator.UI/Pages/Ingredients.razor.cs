using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CateringCalculator.UI.Pages;

public partial class Ingredients {
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private List<Ingredient>? _ingredients;
    private Ingredient _editingIngredient = new();
    private bool _showModal = false;

    protected override async Task OnInitializedAsync() {
        await LoadIngredientsAsync();
    }

    private async Task LoadIngredientsAsync() {
        _ingredients = await RecipeRepository.GetAllIngredientsAsync();
    }

    private void OpenAddModal() {
        _editingIngredient = new Ingredient {
            Id = Guid.NewGuid(), // Direkt eine neue ID vergeben
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

    private async Task DeleteIngredientAsync(Ingredient ingredient) {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Möchtest du die Zutat '{ingredient.Name}' wirklich unwiderruflich löschen?");
        if (!confirmed)
            return;

        try {
            await RecipeRepository.DeleteIngredientAsync(ingredient.Id);
            await LoadIngredientsAsync();
        } catch (InvalidOperationException ex) {
            await JSRuntime.InvokeVoidAsync("alert", ex.Message);
        }
    }
}