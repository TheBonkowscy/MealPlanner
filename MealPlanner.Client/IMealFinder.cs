using MealPlanner.Shared.Meals;

namespace MealPlanner.Client;

public interface IMealFinder
{
    Task<GetMealsResponse> Get(string? query, CancellationToken cancellationToken = default);
}