using System.Globalization;
using System.Net.Http.Json;
using Flurl;
using MealPlanner.Client.Extensions;
using MealPlanner.Client.Models;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;

namespace MealPlanner.Client.Meals;

public class MealClient(HttpClient httpClient) : IAddMeals, IUpdateMeals, IDeleteMeals
{
    public async Task<ApiResult> Add(DateOnly date, CreateMealRequest createMealRequest, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(BuildMenuPath(date), createMealRequest, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiResult.Success();
        }
        
        return await response.ToApiResult(cancellationToken);
    }

    public async Task<ApiResult> Update(DateOnly date, int mealId, UpdateMealRequest updateMealRequest, CancellationToken cancellationToken)
    {
        var endpoint = BuildMenuPath(date).AppendPathSegment(mealId);
        var response = await httpClient.PutAsJsonAsync(endpoint, updateMealRequest, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiResult.Success();
        }
        
        return await response.ToApiResult(cancellationToken);
    }

    public async Task<ApiResult> Delete(DateOnly date, int mealId, CancellationToken cancellationToken)
    {
        var endpoint = BuildMenuPath(date).AppendPathSegment(mealId);
        var response = await httpClient.DeleteAsync(endpoint, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return ApiResult.Success();
        }
        
        return await response.ToApiResult(cancellationToken);
    }

    private static string BuildMenuPath(DateOnly date)
    {
        return Constants.MenusRoute.AppendPathSegment(date.ToString("O", CultureInfo.InvariantCulture))
            .AppendPathSegment("meals");
    }
}