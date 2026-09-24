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
        var vodka = new Ingredient { Id = Guid.NewGuid(), Name = "Wodka", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 15.99m };
        var tequila = new Ingredient { Id = Guid.NewGuid(), Name = "Tequila", Unit = IngredientUnit.Milliliter, PackageSize = 700m, PackagePrice = 18.99m };
        var gin = new Ingredient { Id = Guid.NewGuid(), Name = "Gin", Unit = IngredientUnit.Milliliter, PackageSize = 700m, PackagePrice = 19.99m };
        var tripleSec = new Ingredient { Id = Guid.NewGuid(), Name = "Triple Sec (Orangenlikör)", Unit = IngredientUnit.Milliliter, PackageSize = 700m, PackagePrice = 12.99m };
        var peachSchnapps = new Ingredient { Id = Guid.NewGuid(), Name = "Pfirsichlikör", Unit = IngredientUnit.Milliliter, PackageSize = 700m, PackagePrice = 11.99m };

        var lime = new Ingredient { Id = Guid.NewGuid(), Name = "Limetten", Unit = IngredientUnit.Piece, PackageSize = 5m, PackagePrice = 2.49m };
        var limeJuice = new Ingredient { Id = Guid.NewGuid(), Name = "Limettensaft (Flasche)", Unit = IngredientUnit.Milliliter, PackageSize = 750m, PackagePrice = 3.99m };
        var brownSugar = new Ingredient { Id = Guid.NewGuid(), Name = "Brauner Rohrzucker", Unit = IngredientUnit.Gram, PackageSize = 500m, PackagePrice = 3.49m };
        var mint = new Ingredient { Id = Guid.NewGuid(), Name = "Frische Minze", Unit = IngredientUnit.Piece, PackageSize = 10m, PackagePrice = 1.29m };

        var cola = new Ingredient { Id = Guid.NewGuid(), Name = "Cola", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 1.29m };
        var soda = new Ingredient { Id = Guid.NewGuid(), Name = "Soda Water", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 0.99m };
        var tonic = new Ingredient { Id = Guid.NewGuid(), Name = "Tonic Water", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 1.49m };
        var gingerBeer = new Ingredient { Id = Guid.NewGuid(), Name = "Ginger Beer", Unit = IngredientUnit.Milliliter, PackageSize = 750m, PackagePrice = 2.29m };

        var orangeJuice = new Ingredient { Id = Guid.NewGuid(), Name = "Orangensaft", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 1.99m };
        var pineappleJuice = new Ingredient { Id = Guid.NewGuid(), Name = "Ananassaft", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 2.19m };
        var cranberryJuice = new Ingredient { Id = Guid.NewGuid(), Name = "Cranberrysaft", Unit = IngredientUnit.Milliliter, PackageSize = 1000m, PackagePrice = 2.49m };
        var coconutCream = new Ingredient { Id = Guid.NewGuid(), Name = "Kokoscreme / -sirup", Unit = IngredientUnit.Milliliter, PackageSize = 500m, PackagePrice = 4.99m };
        var grenadine = new Ingredient { Id = Guid.NewGuid(), Name = "Grenadine (Sirup)", Unit = IngredientUnit.Milliliter, PackageSize = 750m, PackagePrice = 4.49m };

        var ice = new Ingredient { Id = Guid.NewGuid(), Name = "Eiswürfel", Unit = IngredientUnit.Gram, PackageSize = 2000m, PackagePrice = 2.99m };

        context.Ingredients.AddRange(
            rumWhite, cachaça, vodka, tequila, gin, tripleSec, peachSchnapps,
            lime, limeJuice, brownSugar, mint, cola, soda, tonic, gingerBeer,
            orangeJuice, pineappleJuice, cranberryJuice, coconutCream, grenadine, ice
        );

        var caipi = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Caipirinha",
            Description = "Der brasilianische Klassiker mit Cachaça und Limette.",
            Items = [
                new() { IngredientId = cachaça.Id, Amount = 60m },
                new() { IngredientId = lime.Id, Amount = 1m },
                new() { IngredientId = brownSugar.Id, Amount = 10m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var mojito = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Mojito",
            Description = "Erfrischender Kubaner mit Rum, Minze und Soda.",
            Items = [
                new() { IngredientId = rumWhite.Id, Amount = 50m },
                new() { IngredientId = lime.Id, Amount = 0.5m },
                new() { IngredientId = brownSugar.Id, Amount = 10m },
                new() { IngredientId = mint.Id, Amount = 1m },
                new() { IngredientId = soda.Id, Amount = 200m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var cubaLibre = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Cuba Libre",
            Description = "Rum trifft auf Cola und frische Limette.",
            Items = [
                new() { IngredientId = rumWhite.Id, Amount = 50m },
                new() { IngredientId = lime.Id, Amount = 0.5m },
                new() { IngredientId = cola.Id, Amount = 150m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var pinaColada = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Piña Colada",
            Description = "Die tropische Cream-Variante mit Ananas und Kokos.",
            Items = [
                new() { IngredientId = rumWhite.Id, Amount = 60m },
                new() { IngredientId = pineappleJuice.Id, Amount = 120m },
                new() { IngredientId = coconutCream.Id, Amount = 40m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var margarita = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Margarita",
            Description = "Mexikanischer Tequila-Klassiker mit Triple Sec.",
            Items = [
                new() { IngredientId = tequila.Id, Amount = 50m },
                new() { IngredientId = tripleSec.Id, Amount = 25m },
                new() { IngredientId = limeJuice.Id, Amount = 25m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var sexOnTheBeach = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Sex on the Beach",
            Description = "Fruchtiger Sommer-Cocktail mit Wodka und Pfirsichlikör.",
            Items = [
                new() { IngredientId = vodka.Id, Amount = 40m },
                new() { IngredientId = peachSchnapps.Id, Amount = 20m },
                new() { IngredientId = orangeJuice.Id, Amount = 80m },
                new() { IngredientId = cranberryJuice.Id, Amount = 80m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var moscowMule = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Moscow Mule",
            Description = "Wodka mit würzigem Ginger Beer und Limette.",
            Items = [
                new() { IngredientId = vodka.Id, Amount = 50m },
                new() { IngredientId = limeJuice.Id, Amount = 20m },
                new() { IngredientId = gingerBeer.Id, Amount = 150m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var ginTonic = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Gin Tonic",
            Description = "Der schlichte, elegante Longdrink-Klassiker.",
            Items = [
                new() { IngredientId = gin.Id, Amount = 50m },
                new() { IngredientId = tonic.Id, Amount = 150m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var tequilaSunrise = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Tequila Sunrise",
            Description = "Optisch ein Highlight durch den Grenadine-Farbverlauf.",
            Items = [
                new() { IngredientId = tequila.Id, Amount = 50m },
                new() { IngredientId = orangeJuice.Id, Amount = 150m },
                new() { IngredientId = grenadine.Id, Amount = 15m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        var daiquiri = new Recipe {
            Id = Guid.NewGuid(),
            Name = "Daiquiri",
            Description = "Puristisch, frisch und erfrischend aus Rum und Limette.",
            Items = [
                new() { IngredientId = rumWhite.Id, Amount = 60m },
                new() { IngredientId = limeJuice.Id, Amount = 30m },
                new() { IngredientId = brownSugar.Id, Amount = 10m },
                new() { IngredientId = ice.Id, Amount = 150m }
            ]
        };

        context.Recipes.AddRange(
            caipi, mojito, cubaLibre, pinaColada, margarita,
            sexOnTheBeach, moscowMule, ginTonic, tequilaSunrise, daiquiri
        );

        // --- 3. Beispiel-Event für den Start ---
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
            SelectedRecipes = [
                new() { RecipeId = caipi.Id, Percentage = 30m },
                new() { RecipeId = mojito.Id, Percentage = 30m },
                new() { RecipeId = sexOnTheBeach.Id, Percentage = 40m }
            ],
            FixedCostItems = [
                new() { Description = "Musikanlage & Licht", Amount = 100.00m },
                new() { Description = "Deko", Amount = 50.00m }
            ],
            PersonnelCostItems = [
                new() { RoleName = "Barkeeper", Count = 2, Hours = 5, HourlyRate = 15.00m }
            ]
        };

        context.EventPlans.Add(sampleEvent);

        await context.SaveChangesAsync();
    }
}