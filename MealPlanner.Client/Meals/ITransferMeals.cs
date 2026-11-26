using MealPlanner.Client.Models;
using MealPlanner.Shared.Menus.Meals;

namespace MealPlanner.Client.Meals;

public interface ITransferMeals
{
    Task<ApiResult> Transfer(DateOnly date, int mealId, TransferMealRequest transferMealRequest, CancellationToken cancellationToken);
}
