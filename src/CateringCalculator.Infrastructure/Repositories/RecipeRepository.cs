using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Repositories;

public class RecipeRepository(AppDbContext context) : IRecipeRepository {
    public async Task<List<Recipe>> GetAllRecipesAsync() {
        return await context.Recipes
            .Include(r => r.Items)
            .ThenInclude(i => i.Ingredient)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id) {
        return await context.Recipes
            .Include(r => r.Items)
            .ThenInclude(i => i.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddRecipeAsync(Recipe recipe) {
        foreach (var item in recipe.Items) {
            if (item.IngredientId != Guid.Empty) {
                item.Ingredient = null!;
            }
        }
        await context.Recipes.AddAsync(recipe);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRecipeAsync(Recipe recipe) {
        // 1. Rezept-Stammdaten laden
        var existingRecipe = await context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (existingRecipe == null) {
            throw new KeyNotFoundException($"Rezept mit ID {recipe.Id} wurde nicht gefunden.");
        }

        // 2. Werte des Hauptobjekts (Name, Description etc.) aktualisieren
        context.Entry(existingRecipe).CurrentValues.SetValues(recipe);

        // 3. Alle BESHENENDEN RecipeItems für dieses Rezept aus der DB laden
        var existingItems = await context.RecipeItems
            .Where(i => EF.Property<Guid>(i, "RecipeId") == recipe.Id) // Funktioniert auch wenn RecipeId eine Shadow Property ist
            .ToListAsync();

        // 4. Alte Items komplett entfernen
        context.RecipeItems.RemoveRange(existingItems);

        // 5. Neue Items sauber aufbauen (ohne Altlasten/Tracking-Leichen)
        foreach (var item in recipe.Items) {
            var newItem = new RecipeItem {
                Id = Guid.NewGuid(), // Neue ID vergeben, um Concurrency-Konflikte auszuschließen
                IngredientId = item.IngredientId,
                Amount = item.Amount
            };

            // Shadow Property RecipeId explizit setzen, falls vorhanden
            context.Entry(newItem).Property("RecipeId").CurrentValue = recipe.Id;

            context.RecipeItems.Add(newItem);
        }

        // 6. Speichern
        await context.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(Guid id) {
        var recipe = await context.Recipes
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe != null) {
            context.RecipeItems.RemoveRange(recipe.Items);
            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Ingredient>> GetAllIngredientsAsync() {
        return await context.Ingredients.AsNoTracking().ToListAsync();
    }

    public async Task AddIngredientAsync(Ingredient ingredient) {
        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync();
    }

    public async Task UpdateIngredientAsync(Ingredient ingredient) {
        var existing = await context.Ingredients.FirstOrDefaultAsync(i => i.Id == ingredient.Id);
        if (existing != null) {
            context.Entry(existing).CurrentValues.SetValues(ingredient);
            await context.SaveChangesAsync();
        }
    }
}