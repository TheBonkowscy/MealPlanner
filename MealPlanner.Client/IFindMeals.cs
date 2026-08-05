using MealPlanner.Shared.Meals;

namespace MealPlanner.Client;

public interface IFindMeals
{
    Task<GetMealsResponse> Get(string? query, CancellationToken cancellationToken = default);
}