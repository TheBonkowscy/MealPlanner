using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes;

namespace MealPlanner.Client;

internal class MealPlannerClient(HttpClient httpClient) : IFindMenus, ICreateMenus, IFindRecipes, IUpdateMenus, IDeleteMenus, IFindIngredients
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
            return await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        }

        return GetExistingMenusResponse.Empty;
    }

    // TODO: this should probably live in a separate client
    public async Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute;
        if (!string.IsNullOrWhiteSpace(query))
        {
            endpoint = endpoint.AppendQueryParam("q", query);
        }

        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetRecipesResponse?>();
        }

        return GetRecipesResponse.Empty;
    }

    public async Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PutAsJsonAsync(AppendDateToBaseUri(request.Date.ToString("yyyy-MM-dd")), request, options: null,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UpdateMenuResponse>();
        }

        throw new Exception("Unable to update menu");   // TODO: concrete types?
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

    public async Task<GetIngredientsResponse> Get(CancellationToken cancellationToken = default)
    {
        const string endpoint = Constants.IngredientsRoute;
        
        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetIngredientsResponse>();
        }

        return GetIngredientsResponse.Empty;
    }
}