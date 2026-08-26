using MealPlanner.Domain.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Services.Recipes.Preparations;

public interface IUpdateRecipePreparationsInfo
{
    Task<Result<GetRecipeDetailsResponse>> UpdatePreparations(int recipeId,
        int preparationId,
        UpdateRecipePreparationsInfoRequest request,
        CancellationToken cancellationToken);
}

public class RecipePreparationsInfoUpdater : IUpdateRecipePreparationsInfo
{
    public Task<Result<GetRecipeDetailsResponse>> UpdatePreparations(int recipeId,
        int preparationId,
        UpdateRecipePreparationsInfoRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}