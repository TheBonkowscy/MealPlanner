using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Services.Recipes.Steps;

public interface ICreateRecipeStep
{
    Task<GetRecipeDetailsResponse> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepCreator : ICreateRecipeStep
{
    public Task<GetRecipeDetailsResponse> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}