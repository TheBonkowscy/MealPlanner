using MealPlanner.Shared.Meals;

namespace MealPlanner.Client;

public interface IMealFinder
{
    Task<GetMealsResponse> FindMeals(string query, CancellationToken cancellationToken);
}