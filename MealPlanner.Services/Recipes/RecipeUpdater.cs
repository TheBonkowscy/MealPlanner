using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IUpdateRecipe
{
    Task<Result<GetRecipeDetailsResponse>> Update(int recipeId, UpdateRecipeRequest request, CancellationToken cancellationToken);
}

public class RecipeUpdater(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IUpdateRecipe
{
    public async Task<Result<GetRecipeDetailsResponse>> Update(int recipeId, UpdateRecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Steps)
            .Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        
        if (recipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
        }

        List<Result> updateResults = [recipe.UpdateName(request.Name), recipe.UpdateServings(request.Servings)];
        var errors = updateResults.AllErrors();
        if (errors.Count != 0)
        {
            return Result.Failure<GetRecipeDetailsResponse>(errors);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);
        
        return Result.Success(recipeMapper.ToDetails(recipe));
    }
}