using MealPlanner.Shared.Recipes;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client;

public interface IFindRecipes
{
    Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken = default);
}