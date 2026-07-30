using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

internal class MealPlannerClient(HttpClient httpClient) : IMenuClient
{
    public async Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(Constants.MenuRoute, createMenuRequest, options: null,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CreateMenuResponse>();
        }

        throw new Exception("Unable to create menu");   // TODO: concrete types?
    }

    public async Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenuRoute.AppendPathSegment(id.ToString());
        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetMenuResponse>(cancellationToken); 
        }

        return null;
    }

    public async Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenuRoute.AppendPathSegment(date.ToString("yyyy-MM-dd"));
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

    public async Task<GetExistingMenusResponse> GetRange(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenuRoute;
        
        if (from is not null)
        {
            endpoint = endpoint.AppendQueryParam("from", from.Value.ToUniversalTime());
        }

        if (to is not null)
        {
            endpoint = endpoint.AppendQueryParam("to", to.Value.ToUniversalTime());
        }

        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        }

        return GetExistingMenusResponse.Empty;
    }
}