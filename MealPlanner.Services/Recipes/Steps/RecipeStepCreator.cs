using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Steps;

public interface ICreateRecipeStep
{
    Task<Result<GetRecipeDetailsResponse>> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepCreator(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : ICreateRecipeStep
{
    public async Task<Result<GetRecipeDetailsResponse>> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes
            .Include(recipe => recipe.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (recipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
        }
        
        var result = recipe.AddStep(request.Order, request.Instructions);
        if (result.IsFailure)
        {
            return Result.Failure<GetRecipeDetailsResponse>(result.Errors);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);

        return Result.Success(recipeMapper.ToDetails(recipe));
    }
}