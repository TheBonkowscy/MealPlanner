using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Preparations;

public interface ICreateRecipePreparations
{
    Task<Result<GetRecipeDetailsResponse>> CreatePreparations(int recipeId, 
        CreateRecipePreparationsRequest request,
        CancellationToken cancellationToken);
}

public class RecipePreparationsCreator(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : ICreateRecipePreparations
{
    public async Task<Result<GetRecipeDetailsResponse>> CreatePreparations(int recipeId, CreateRecipePreparationsRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Preparations)
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
        }

        var result = recipe.AddPreparations(request.Description, request.LeadDays, request.Required);
        if (result.IsFailure)
        {
            return Result.Failure<GetRecipeDetailsResponse>(result.Errors);
        }

        await ctx.SaveChangesAsync(cancellationToken);

        return Result.Success(recipeMapper.ToDetails(recipe));
    }
}