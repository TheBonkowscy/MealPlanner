using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Steps;

public interface IUpdateRecipeStep
{
    Task<GetRecipeDetailsResponse> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepUpdater(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IUpdateRecipeStep
{
    public async Task<GetRecipeDetailsResponse> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes
            .Include(recipe => recipe.Steps)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        if (recipe is null)
        {
            throw new InvalidOperationException("Recipe could not be found");   // TODO: custom exceptions?
        }

        recipe.UpdateStep(stepId, request.Order, request.Instructions);
        await ctx.SaveChangesAsync(cancellationToken);

        return recipeMapper.ToDetails(recipe);
    }
}