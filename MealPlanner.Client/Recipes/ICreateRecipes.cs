using MealPlanner.Client.Models;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

public interface ICreateRecipes
{
    Task<ApiResult<CreateRecipeResponse>> CreateRecipe(CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken);
}