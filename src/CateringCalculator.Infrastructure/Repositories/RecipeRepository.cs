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
        context.Recipes.Update(recipe);
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
}