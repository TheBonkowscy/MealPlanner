using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Preparations;

public interface IUpdateRecipePreparations
{
    Task<Result<GetRecipeDetailsResponse>> UpdatePreparations(int recipeId,
        int preparationId,
        UpdateRecipePreparationsRequest request,
        CancellationToken cancellationToken);
}

public class RecipePreparationsUpdater(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IUpdateRecipePreparations
{
    public async Task<Result<GetRecipeDetailsResponse>> UpdatePreparations(int recipeId,
        int preparationId,
        UpdateRecipePreparationsRequest request,
        CancellationToken cancellationToken)
    {
        var existingRecipe = await ctx.Recipes.Include(x => x.Preparations)
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        if (existingRecipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
        }

        var result =
            existingRecipe.UpdatePreparations(preparationId, request.Description, request.LeadDays, request.Required);
        if (result.IsFailure)
        {
            return Result.Failure<GetRecipeDetailsResponse>(result.Errors);
        }

        await ctx.SaveChangesAsync(cancellationToken);

        return Result.Success(recipeMapper.ToDetails(existingRecipe));
    }
}