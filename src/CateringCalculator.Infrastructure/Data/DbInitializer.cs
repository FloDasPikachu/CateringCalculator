using CateringCalculator.Core.Enums;
using CateringCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Data;

public static class DbInitializer {
    public static async Task InitializeAsync(AppDbContext context) {
        if (await context.Ingredients.AnyAsync()) {
            return;
        }

        var rumWhite = new Ingredient { Id = Guid.NewGuid(), Name = "Weißer Rum", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 14.99m };
        var cachaça = new Ingredient { Id = Guid.NewGuid(), Name = "Cachaça", Unit = IngredientUnit.Milliliter, PackageSize = 700m, PackagePrice = 13.50m };
        var lime = new Ingredient { Id = Guid.NewGuid(), Name = "Limetten", Unit = IngredientUnit.Piece, PackageSize = 5m, PackagePrice = 2.49m };
        var brownSugar = new Ingredient { Id = Guid.NewGuid(), Name = "Brauner Rohrzucker", Unit = IngredientUnit.Gram, PackageSize = 500m, PackagePrice = 3.49m };
        var mint = new Ingredient { Id = Guid.NewGuid(), Name = "Frische Minze", Unit = IngredientUnit.Piece, PackageSize = 10m, PackagePrice = 1.29m };
        var soda = new Ingredient { Id = Guid.NewGuid(), Name = "Soda Water", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 0.99m };
        var ice = new Ingredient { Id = Guid.NewGuid(), Name = "Eiswürfel", Unit = IngredientUnit.Gram, PackageSize = 2000m, PackagePrice = 2.99m };

        context.Ingredients.AddRange(rumWhite, cachaça, lime, brownSugar, mint, soda, ice);

        var caipi = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Caipirinha",
            Items = new List<RecipeItem> {
                new() { IngredientId = cachaça.Id, Amount = 60m },
                new() { IngredientId = lime.Id, Amount = 1m },
                new() { IngredientId = brownSugar.Id, Amount = 10m },
                new() { IngredientId = ice.Id, Amount = 150m }
            }
        };

        var mojito = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Mojito",
            Items = new List<RecipeItem> {
                new() { IngredientId = rumWhite.Id, Amount = 50m },
                new() { IngredientId = lime.Id, Amount = 0.5m },
                new() { IngredientId = brownSugar.Id, Amount = 10m },
                new() { IngredientId = mint.Id, Amount = 1m },
                new() { IngredientId = soda.Id, Amount = 200m },
                new() { IngredientId = ice.Id, Amount = 150m }
            }
        };

        context.Recipes.AddRange(caipi, mojito);

        var sampleEvent = new EventPlan {
            Id = Guid.NewGuid(),
            Title = "Sommer-Opening 2026",
            GuestCount = 50,
            AverageDrinksPerGuest = 3,
            WasteBufferPercent = 10m,
            FixedCosts = 150.00m,
            PersonnelCosts = 150.00m,
            TargetProfit = 250.00m,
            FreeDrinksCount = 10,
            SelectedRecipes = new List<EventRecipe> {
                new() { RecipeId = caipi.Id, Percentage = 60m },
                new() { RecipeId = mojito.Id, Percentage = 40m }
            },
            FixedCostItems = new List<FixedCostItem> {
                new() { Description = "Musikanlage & Licht", Amount = 100.00m },
                new() { Description = "Deko", Amount = 50.00m }
            },
            PersonnelCostItems = new List<PersonnelCostItem> {
                new() { RoleName = "Barkeeper", Count = 2, Hours = 5, HourlyRate = 15.00m }
            }
        };

        context.EventPlans.Add(sampleEvent);

        await context.SaveChangesAsync();
    }
}