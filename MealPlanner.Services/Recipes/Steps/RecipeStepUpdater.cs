using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Services.Recipes.Steps;

public interface IUpdateRecipeStep
{
    Task<GetRecipeDetailsResponse> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepUpdater : IUpdateRecipeStep
{
    public Task<GetRecipeDetailsResponse> UpdateStep(int recipeId, int stepId, UpdateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}