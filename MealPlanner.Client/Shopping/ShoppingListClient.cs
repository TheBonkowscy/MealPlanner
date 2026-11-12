using Flurl;
using MealPlanner.Client.Extensions;
using MealPlanner.Client.Models;
using MealPlanner.Shared.Shopping;
using MealPlanner.Shared.Shopping.Responses;

namespace MealPlanner.Client.Shopping;

internal class ShoppingListClient(HttpClient httpClient) : IFindShoppingLists
{
    public async Task<ApiResult<ShoppingListResponse>> Get(
        DateOnly startDate,
        PlanningStrategyKind strategy,
        CancellationToken cancellationToken)
    {
        var endpoint = Constants.ShoppingListsRoute
            .AppendQueryParam("startDate", startDate.ToString("yyyy-MM-dd"))
            .AppendQueryParam("strategy", strategy.ToString());

        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        return await response.ToApiResult<ShoppingListResponse>(cancellationToken);
    }
}
