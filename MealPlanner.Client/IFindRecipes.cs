using MealPlanner.Shared.Recipes;

namespace MealPlanner.Client;

public interface IFindRecipes
{
    Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken = default);
}