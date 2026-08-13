using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client;

public interface ICreateRecipes
{
    Task<CreateRecipeResponse> CreateRecipe(CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken);
}