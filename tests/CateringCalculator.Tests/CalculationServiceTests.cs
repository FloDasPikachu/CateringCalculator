using CateringCalculator.Core.Models;
using CateringCalculator.Core.Services;
using Xunit;

namespace CateringCalculator.Tests;

public class CalculationServiceTests {
    private readonly CalculationService _sut; // System Under Test

    public CalculationServiceTests() {
        _sut = new CalculationService();
    }

    [Fact]
    public void Calculate_WithNullEventPlan_ThrowsArgumentNullException() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Calculate(null!));
    }

    [Fact]
    public void Calculate_WithEmptyRecipes_ReturnsZeroTotals() {
        // Arrange
        var eventPlan = new EventPlan {
            Title = "Test Event ohne Rezepte",
            GuestCount = 50,
            AverageDrinksPerGuest = 3
        };

        // Act
        var result = _sut.Calculate(eventPlan);

        // Assert
        Assert.Equal(0, result.TotalMaterialCost);
        Assert.Equal(0, result.TotalIceInKg);
        Assert.Empty(result.ShoppingList);
    }

    [Fact]
    public void Calculate_CorrectlyCalculatesBottleCeilingAndCostsWithWasteBuffer() {
        // Arrange: Gin & Tonic für 10 Personen (je 1 Drink)
        // Gin: 50 ml pro Drink | Flasche: 700 ml @ 20.00 €
        // Tonic: 150 ml pro Drink | Flasche: 1000 ml @ 2.50 €
        // Schwundpuffer: 10 %

        var gin = new Ingredient {
            Name = "Dry Gin",
            BottleVolumeMl = 700,
            BottlePrice = 20.00m
        };

        var tonic = new Ingredient {
            Name = "Tonic Water",
            BottleVolumeMl = 1000,
            BottlePrice = 2.50m
        };

        var ginTonicRecipe = new Recipe {
            Name = "Gin & Tonic",
            IceInGrams = 150,
            Items = new List<RecipeItem>
            {
                new RecipeItem { Ingredient = gin, AmountMl = 50 },
                new RecipeItem { Ingredient = tonic, AmountMl = 150 }
            }
        };

        var eventPlan = new EventPlan {
            Title = "Sommerfest",
            GuestCount = 10,
            AverageDrinksPerGuest = 1, // Total 10 Drinks
            WasteBufferPercent = 10m,  // +10 % Puffer
            SelectedRecipes = new List<Recipe> { ginTonicRecipe }
        };

        // Act
        var result = _sut.Calculate(eventPlan);

        // Assert
        // Gin netto: 10 * 50ml = 500ml | brutto (+10%): 550ml -> 1 Flasche (700ml)
        var ginShoppingItem = result.ShoppingList.Single(x => x.IngredientName == "Dry Gin");
        Assert.Equal(550m, ginShoppingItem.TotalVolumeNeededMl);
        Assert.Equal(1, ginShoppingItem.BottlesToBuy);
        Assert.Equal(20.00m, ginShoppingItem.TotalCost);

        // Tonic netto: 10 * 150ml = 1500ml | brutto (+10%): 1650ml -> 2 Flaschen (2000ml)
        var tonicShoppingItem = result.ShoppingList.Single(x => x.IngredientName == "Tonic Water");
        Assert.Equal(1650m, tonicShoppingItem.TotalVolumeNeededMl);
        Assert.Equal(2, tonicShoppingItem.BottlesToBuy);
        Assert.Equal(5.00m, tonicShoppingItem.TotalCost);

        // Eis: 10 * 150g = 1500g | brutto (+10%): 1650g = 1.65 kg
        Assert.Equal(1.65m, result.TotalIceInKg);

        // Gesamtkosten: 1x Gin (20.00) + 2x Tonic (5.00) = 25.00 €
        Assert.Equal(25.00m, result.TotalMaterialCost);
    }
}