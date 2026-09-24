using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using Microsoft.AspNetCore.Components;

namespace CateringCalculator.UI.Components;

public partial class RecipeModal {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Recipe Recipe { get; set; } = new();
    [Parameter] public List<Ingredient> AvailableIngredients { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public EventCallback<Recipe> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<Ingredient> OnIngredientAdded { get; set; }

    private Guid _selectedIngredientId = Guid.Empty;
    private decimal _amount = 50m;
    private string _amountStep = "5";
    private bool _showNewIngredientInput = false;

    private string _newIngName = "";
    private IngredientUnit _newIngUnit = IngredientUnit.Milliliter;
    private decimal _newIngSize = 700m;
    private decimal _newIngPrice = 12.99m;

    private void OnIngredientSelectionChanged(ChangeEventArgs e) {
        if (Guid.TryParse(e.Value?.ToString(), out var parsedId)) {
            _selectedIngredientId = parsedId;
            var selectedIng = AvailableIngredients.FirstOrDefault(i => i.Id == _selectedIngredientId);

            if (selectedIng != null) {
                switch (selectedIng.Unit) {
                    case IngredientUnit.Piece:
                        _amount = 1m;
                        _amountStep = "0.5";
                        break;
                    case IngredientUnit.Gram:
                        _amount = 10m;
                        _amountStep = "1";
                        break;
                    case IngredientUnit.Milliliter:
                    default:
                        _amount = 50m;
                        _amountStep = "5";
                        break;
                }
            }
        } else {
            _selectedIngredientId = Guid.Empty;
        }
    }

    private void AddSelectedIngredientToRecipe() {
        if (_selectedIngredientId == Guid.Empty || _amount <= 0)
            return;

        var existingItem = Recipe.Items.FirstOrDefault(i => i.IngredientId == _selectedIngredientId);
        if (existingItem != null) {
            existingItem.Amount += _amount;
        } else {
            var selectedIng = AvailableIngredients.FirstOrDefault(i => i.Id == _selectedIngredientId);
            Recipe.Items.Add(new RecipeItem {
                Id = Guid.Empty,
                IngredientId = _selectedIngredientId,
                Ingredient = selectedIng,
                Amount = _amount
            });
        }

        ResetIngredientInput();
    }

    private void RemoveItemFromRecipe(RecipeItem item) {
        Recipe.Items.Remove(item);
    }

    private void ToggleNewIngredientFields() {
        _showNewIngredientInput = !_showNewIngredientInput;
    }

    private async Task CreateAndAddIngredientAsync() {
        if (string.IsNullOrWhiteSpace(_newIngName) || _newIngSize <= 0 || _newIngPrice <= 0)
            return;

        var newIng = new Ingredient {
            Id = Guid.NewGuid(),
            Name = _newIngName,
            Unit = _newIngUnit,
            PackageSize = _newIngSize,
            PackagePrice = _newIngPrice
        };

        await OnIngredientAdded.InvokeAsync(newIng);

        _selectedIngredientId = newIng.Id;
        _showNewIngredientInput = false;
        _newIngName = "";

        AddSelectedIngredientToRecipe();
    }

    private void ResetIngredientInput() {
        _selectedIngredientId = Guid.Empty;
        _amount = 50m;
        _amountStep = "5";
    }

    private string GetSelectedIngredientUnitLabel() {
        var ing = AvailableIngredients.FirstOrDefault(i => i.Id == _selectedIngredientId);
        return ing != null ? GetUnitSuffix(ing.Unit) : "ml/Stk/g";
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
}