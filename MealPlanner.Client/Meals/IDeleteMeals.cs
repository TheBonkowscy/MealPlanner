using MealPlanner.Client.Models;

namespace MealPlanner.Client.Meals;

public interface IDeleteMeals
{
    Task<ApiResult> Delete(DateOnly date, int mealId, CancellationToken cancellationToken);
}