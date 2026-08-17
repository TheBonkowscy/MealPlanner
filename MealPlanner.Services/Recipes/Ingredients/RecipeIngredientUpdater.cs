using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Ingredients;

public interface IUpdateRecipeIngredient
{
    Task<GetRecipeDetailsResponse> UpdateIngredient(int recipeId, int ingredientId, AddIngredientRequest request, CancellationToken cancellationToken);
}

public class RecipeIngredientUpdater(MealPlannerDbContext ctx,
    MeasureUnitMapper measureUnitMapper,
    RecipeMapper recipeMapper) : IUpdateRecipeIngredient
{
    public async Task<GetRecipeDetailsResponse> UpdateIngredient(int recipeId, int ingredientId, AddIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Ingredients)
            .ThenInclude(x => x.IngredientId)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            throw new InvalidOperationException($"Recipe could not be found");   // TODO: custom exceptions?
        }
        
        var ingredient = recipe.GetIngredient(ingredientId, measureUnitMapper.Map(request.Unit));
        if (ingredient is null)
        {
            throw new InvalidOperationException($"Specified ingredient could not be found");   // TODO: custom exceptions?
        }
        
        ingredient.UpdateQuantity(request.Quantity);
        await ctx.SaveChangesAsync(cancellationToken);

        return recipeMapper.ToDetails(recipe);
    }
}