using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client.Menus;

internal class MenuClient(HttpClient httpClient) : IFindMenus, IDeleteMenus
{
    public async Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken)
    {
        var endpoint = AppendDateToBaseUri(id.ToString());
        var response = await httpClient.GetAsync(endpoint, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetMenuResponse>(cancellationToken); 
        }

        return null;
    }

    public async Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken)
    {
        var endpoint = AppendDateToBaseUri(date.ToString("yyyy-MM-dd"));
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
            return await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>(cancellationToken);
        }

        return GetExistingMenusResponse.Empty;
    }

    public async Task<GetWeekResponse> GetWeek(DateOnly date, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenuWeeksRoute.AppendPathSegment(date.ToString("O"));
        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetWeekResponse>(cancellationToken);
        }

        return GetWeekResponse.Empty;
    }

    public async Task<bool> Delete(DateOnly date, CancellationToken cancellationToken)
    {
        var endpoint = Constants.MenusRoute.AppendPathSegment(date.ToString("O"));
        var result = await httpClient.DeleteAsync(endpoint, cancellationToken);
        return result.IsSuccessStatusCode;
    }

    private static Url AppendDateToBaseUri(string toString)
    {
        return Constants.MenusRoute.AppendPathSegment(toString);
    }
}