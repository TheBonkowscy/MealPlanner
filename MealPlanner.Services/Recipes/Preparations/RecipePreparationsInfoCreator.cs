using MealPlanner.Domain.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Services.Recipes.Preparations;

public interface ICreateRecipePreparationsInfo
{
    Task<Result<GetRecipeDetailsResponse>> CreatePreparations(int recipeId, 
        CreateRecipePreparationsInfoRequest request,
        CancellationToken cancellationToken);
}

public class RecipePreparationsInfoCreator : ICreateRecipePreparationsInfo
{
    public Task<Result<GetRecipeDetailsResponse>> CreatePreparations(int recipeId, CreateRecipePreparationsInfoRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}