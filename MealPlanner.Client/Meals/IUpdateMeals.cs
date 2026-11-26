using MealPlanner.Client.Models;
using MealPlanner.Shared.Menus.Requests;

namespace MealPlanner.Client.Meals;

public interface IUpdateMeals
{
    Task<ApiResult> Update(DateOnly date, int mealId, UpdateMealRequest updateMealRequest, CancellationToken cancellationToken);
}