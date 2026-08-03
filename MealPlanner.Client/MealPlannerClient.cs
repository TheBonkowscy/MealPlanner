using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Meals;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

internal class MealPlannerClient(HttpClient httpClient) : IMenuFinder, IMenuCreator, IMealFinder
{
    public async Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(Constants.MenusRoute, createMenuRequest, options: null,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CreateMenuResponse>();
        }

        throw new Exception("Unable to create menu");   // TODO: concrete types?
    }

    public async Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenusRoute.AppendPathSegment(id.ToString());
        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetMenuResponse>(cancellationToken); 
        }

        return null;
    }

    public async Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenusRoute.AppendPathSegment(date.ToString("yyyy-MM-dd"));
        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetMenuResponse>(cancellationToken); 
        }

        return null;
    }

    public async Task<GetMenuResponse?> GetToday(CancellationToken cancellationToken)
    {
        return await Get(DateTime.Today, cancellationToken);
    }

    public async Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenusRoute;
        
        if (from is not null)
        {
            endpoint = endpoint.AppendQueryParam("from", from.Value);
        }

        if (to is not null)
        {
            endpoint = endpoint.AppendQueryParam("to", to.Value);
        }

        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        }

        return GetExistingMenusResponse.Empty;
    }

    public Task<GetMealsResponse> FindMeals(string query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}