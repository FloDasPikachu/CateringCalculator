using CateringCalculator.Core.Interfaces;
using CateringCalculator.Core.Models;
using CateringCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CateringCalculator.Infrastructure.Repositories;

public class RecipeRepository : IRecipeRepository {
    private readonly AppDbContext _context;

    public RecipeRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<List<Recipe>> GetAllRecipesAsync() {
        return await _context.Recipes
            .Include(r => r.Items)
            .ThenInclude(i => i.Ingredient)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id) {
        return await _context.Recipes
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
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRecipeAsync(Recipe recipe) {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(Guid id) {
        var recipe = await _context.Recipes
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe != null) {
            _context.RecipeItems.RemoveRange(recipe.Items);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Ingredient>> GetAllIngredientsAsync() {
        return await _context.Ingredients.AsNoTracking().ToListAsync();
    }

    public async Task AddIngredientAsync(Ingredient ingredient) {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
    }
}