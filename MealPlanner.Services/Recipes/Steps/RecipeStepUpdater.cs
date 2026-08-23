using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Steps;

public interface IUpdateRecipeStep
{
    Task<Result<GetRecipeDetailsResponse>> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepUpdater(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IUpdateRecipeStep
{
    public async Task<Result<GetRecipeDetailsResponse>> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes
            .Include(recipe => recipe.Steps)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        
        if (recipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
        }

        var result = recipe.UpdateStep(stepId, request.Order, request.Instructions);
        if (result.IsFailure)
        {
            return  Result.Failure<GetRecipeDetailsResponse>(result.Errors);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);

        return Result.Success(recipeMapper.ToDetails(recipe));
    }
}