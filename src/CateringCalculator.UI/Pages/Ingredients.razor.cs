using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using CateringCalculator.Resources.Internationalization;
using CateringCalculator.UI.Services;
using CateringCalculator.UI.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CateringCalculator.UI.Pages;

public partial class Ingredients : LocalizedComponentBase, IAsyncDisposable {
    [Inject]
    private PageTitleService TitleService { get; set; } = default!;
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private List<Ingredient>? _ingredients;
    private Ingredient _editingIngredient = new();
    private bool _showModal = false;
    private bool _showInfoModal = false;
    private string _infoMessage = string.Empty;

    private bool _showDeleteConfirmation = false;
    private Ingredient? _ingredientToDelete;

    private ElementReference pageHeaderRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Ingredients>? _dotNetRef;

    protected override async Task OnInitializedAsync() {
        // Localization handled by LocalizedComponentBase
        TitleService.SetTitle("Catering Calculator");
        await LoadIngredientsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/scrollObserver.js");
            await _jsModule.InvokeVoidAsync("observeHeader", pageHeaderRef, _dotNetRef);
        }
    }

    [JSInvokable]
    public void TargetVisibilityChanged(bool isIntersecting) {
        if (isIntersecting) {
            TitleService.SetTitle("Catering Calculator");
        } else {
            TitleService.SetTitle(AppResources.Ingredients_Title);
        }
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

    private void CloseInfoModal() {
        _showInfoModal = false;
        _infoMessage = string.Empty;
    }

    // Localization unsubscribe handled by LocalizedComponentBase.Dispose()

    public async ValueTask DisposeAsync() {
        if (_jsModule != null) {
            await _jsModule.InvokeVoidAsync("unobserveHeader");
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}