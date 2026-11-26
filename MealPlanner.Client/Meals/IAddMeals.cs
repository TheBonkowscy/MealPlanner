using MealPlanner.Client.Models;
using MealPlanner.Shared.Menus.Requests;

namespace MealPlanner.Client.Meals;

public interface IAddMeals
{
    Task<ApiResult> Add(DateOnly date, CreateMealRequest createMealRequest, CancellationToken cancellationToken);
}