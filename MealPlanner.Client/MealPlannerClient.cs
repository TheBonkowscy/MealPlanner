using System.Net.Http.Json;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

internal class MealPlannerClient(HttpClient httpClient) : IMenuClient
{
    public async Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken)
    {
        var result = await httpClient.PostAsJsonAsync(Constants.MenuRoute, createMenuRequest, options: null,
            cancellationToken);

        result.EnsureSuccessStatusCode();
        
        return await result.Content.ReadFromJsonAsync<CreateMenuResponse>();
    }

    public async Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken)
    {
        var endpoint = Path.Combine(Constants.MenuRoute, id.ToString());
        return await httpClient.GetFromJsonAsync<GetMenuResponse>(endpoint, cancellationToken);
    }

    public async Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken)
    {
        var endpoint = Path.Combine(Constants.MenuRoute, date.ToString("yyyy-MM-dd"));
        return await httpClient.GetFromJsonAsync<GetMenuResponse>(endpoint, cancellationToken);
    }

    public async Task<GetMenuResponse?> GetToday(CancellationToken cancellationToken)
    {
        return await Get(DateTime.Today, cancellationToken);
    }
}