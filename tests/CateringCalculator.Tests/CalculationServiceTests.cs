using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using CateringCalculator.Core.Services;
using Xunit;

namespace CateringCalculator.Tests;

public class CalculationServiceTests {
    private readonly CalculationService _sut = new();

    [Fact]
    public void Calculate_WithValidPlanAndMixedUnits_CalculatesCorrectShoppingList() {
        // Arrange
        var rum = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "Weißer Rum",
            Unit = IngredientUnit.Milliliter,
            PackageSize = 700m,   // 700 ml Flasche
            PackagePrice = 14.00m
        };

        var lime = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "Limetten",
            Unit = IngredientUnit.Piece,
            PackageSize = 6m,     // 6er Pack Netz
            PackagePrice = 1.80m
        };

        var sugar = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "Rohrzucker",
            Unit = IngredientUnit.Gram,
            PackageSize = 500m,   // 500g Packung
            PackagePrice = 2.50m
        };

        var recipe = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Mojito",
            Items =
            [
                new() { IngredientId = rum.Id, Ingredient = rum, Amount = 60m },     // 60 ml
                new() { IngredientId = lime.Id, Ingredient = lime, Amount = 0.5m },  // 0.5 Stück
                new() { IngredientId = sugar.Id, Ingredient = sugar, Amount = 10m }  // 10 g
            ]
        };

        var plan = new EventPlan {
            Id = Guid.NewGuid(),
            Title = "Sommerfest",
            GuestCount = 10,
            AverageDrinksPerGuest = 2, // 20 Drinks gesamt
            WasteBufferPercent = 10,  // 10 % Verschnitt
            SelectedRecipes =
            [
                new EventRecipe
                {
                    RecipeId = recipe.Id,
                    Recipe = recipe,
                    Percentage = 100m // 100 % der Drinks entfallen auf den Mojito
                }
            ]
        };

        // Act
        var result = _sut.Calculate(plan);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.TotalDrinksCount);

        // Rum-Berechnung: 20 Drinks * 60 ml = 1200 ml + 10% Waste = 1320 ml -> 2 Flaschen (a 700 ml)
        var rumItem = result.ShoppingList.First(i => i.IngredientId == rum.Id);
        Assert.Equal(1320m, rumItem.TotalAmountNeeded);
        Assert.Equal(2, rumItem.PackagesToBuy);
        Assert.Equal(28.00m, rumItem.TotalCost);
        Assert.Equal(IngredientUnit.Milliliter, rumItem.Unit);

        // Limetten-Berechnung: 20 Drinks * 0.5 Stk = 10 Stk + 10% Waste = 11 Stk -> 2 Netze (a 6 Stk)
        var limeItem = result.ShoppingList.First(i => i.IngredientId == lime.Id);
        Assert.Equal(11m, limeItem.TotalAmountNeeded);
        Assert.Equal(2, limeItem.PackagesToBuy);
        Assert.Equal(3.60m, limeItem.TotalCost);
        Assert.Equal(IngredientUnit.Piece, limeItem.Unit);

        // Zucker-Berechnung: 20 Drinks * 10 g = 200 g + 10% Waste = 220 g -> 1 Packung (a 500 g)
        var sugarItem = result.ShoppingList.First(i => i.IngredientId == sugar.Id);
        Assert.Equal(220m, sugarItem.TotalAmountNeeded);
        Assert.Equal(1, sugarItem.PackagesToBuy);
        Assert.Equal(2.50m, sugarItem.TotalCost);
        Assert.Equal(IngredientUnit.Gram, sugarItem.Unit);
    }

    [Fact]
    public void Calculate_WithEmptyRecipes_ReturnsEmptyShoppingList() {
        // Arrange
        var plan = new EventPlan {
            GuestCount = 50,
            AverageDrinksPerGuest = 2,
            SelectedRecipes = []
        };

        // Act
        var result = _sut.Calculate(plan);

        // Assert
        Assert.Empty(result.ShoppingList);
    }

    [Fact]
    public void Calculate_WithMultipleRecipesAndPercentages_CalculatesWeightedShoppingList() {
        // Arrange
        var rum = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "Weißer Rum",
            Unit = IngredientUnit.Milliliter,
            PackageSize = 700m,
            PackagePrice = 14.00m
        };

        var cachaça = new Ingredient {
            Id = Guid.NewGuid(),
            Name = "Cachaça",
            Unit = IngredientUnit.Milliliter,
            PackageSize = 700m,
            PackagePrice = 15.00m
        };

        var mojito = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Mojito",
            Items = [new() { IngredientId = rum.Id, Ingredient = rum, Amount = 60m }]
        };

        var caipirinha = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Caipirinha",
            Items = [new() { IngredientId = cachaça.Id, Ingredient = cachaça, Amount = 50m }] // 50ml Korrekturwert für Test
        };

        var plan = new EventPlan {
            Id = Guid.NewGuid(),
            Title = "Cocktailabend",
            GuestCount = 10,
            AverageDrinksPerGuest = 2, // 20 Drinks gesamt
            WasteBufferPercent = 0,    // 0% Puffer zur einfacheren Nachvollziehbarkeit im Test
            SelectedRecipes =
            [
                new EventRecipe { RecipeId = mojito.Id, Recipe = mojito, Percentage = 60m },       // 12 Drinks
                new EventRecipe { RecipeId = caipirinha.Id, Recipe = caipirinha, Percentage = 40m } // 8 Drinks
            ]
        };

        // Act
        var result = _sut.Calculate(plan);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.TotalDrinksCount);

        // Rum: 12 Drinks (60% von 20) * 60 ml = 720 ml -> 2 Flaschen (a 700ml, da 1 nicht reicht)
        var rumItem = result.ShoppingList.First(i => i.IngredientId == rum.Id);
        Assert.Equal(720m, rumItem.TotalAmountNeeded);
        Assert.Equal(2, rumItem.PackagesToBuy);

        // Cachaça: 8 Drinks (40% von 20) * 50 ml = 400 ml -> 1 Flasche (a 700ml)
        var cachaçaItem = result.ShoppingList.First(i => i.IngredientId == cachaça.Id);
        Assert.Equal(400m, cachaçaItem.TotalAmountNeeded);
        Assert.Equal(1, cachaçaItem.PackagesToBuy);
    }
}