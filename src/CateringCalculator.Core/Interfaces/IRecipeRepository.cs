using CateringCalculator.Core.Models;

namespace CateringCalculator.Core.Interfaces;

public interface IRecipeRepository {
    Task<List<Recipe>> GetAllRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(Guid id);
    Task AddRecipeAsync(Recipe recipe);
    Task UpdateRecipeAsync(Recipe recipe);
    Task DeleteRecipeAsync(Guid id);
    Task<List<Ingredient>> GetAllIngredientsAsync();
    Task AddIngredientAsync(Ingredient ingredient);
}