using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IUpdateRecipe
{
    Task<GetRecipeDetailsResponse> Update(int recipeId, UpdateRecipeRequest request, CancellationToken cancellationToken);
}

public class RecipeUpdater(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IUpdateRecipe
{
    public async Task<GetRecipeDetailsResponse> Update(int recipeId, UpdateRecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Steps)
            .Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        
        if (recipe is null)
        {
            throw new InvalidOperationException("Recipe could not be found");   // TODO: custom exceptions?
        }

        recipe.UpdateName(request.Name);
        recipe.UpdateServings(request.Servings);
        
        await ctx.SaveChangesAsync(cancellationToken);
        
        return recipeMapper.ToDetails(recipe);
    }
}