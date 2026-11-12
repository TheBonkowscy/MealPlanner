using MealPlanner.Client.Models;
using MealPlanner.Shared.Shopping;
using MealPlanner.Shared.Shopping.Responses;

namespace MealPlanner.Client.Shopping;

public interface IFindShoppingLists
{
    Task<ApiResult<ShoppingListResponse>> Get(DateOnly startDate, PlanningStrategyKind strategy, CancellationToken cancellationToken);
}
