using MealPlanner.Shared.Meals;

namespace MealPlanner.Client;

public interface IFindRecipes
{
    Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken = default);
}