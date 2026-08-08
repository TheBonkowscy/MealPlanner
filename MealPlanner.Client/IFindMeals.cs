using MealPlanner.Shared.Meals;

namespace MealPlanner.Client;

public interface IFindMeals
{
    Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken = default);
}