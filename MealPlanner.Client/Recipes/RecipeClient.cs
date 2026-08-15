using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

internal class RecipeClient(HttpClient httpClient) : 
    IFindRecipes, ICreateRecipes, IDeleteRecipes,
    IFindIngredients
{
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
            return await result.Content.ReadFromJsonAsync<GetRecipesResponse?>(cancellationToken);
        }

        return GetRecipesResponse.Empty;
    }

    public async Task<GetRecipeDetailsResponse?> Get(int id, CancellationToken cancellationToken = default)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id);
        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetRecipeDetailsResponse?>(cancellationToken);
        }

        return null;
    }

    public async Task<GetIngredientsResponse> Get(CancellationToken cancellationToken = default)
    {
        const string endpoint = Constants.IngredientsRoute;
        
        var result = await httpClient.GetAsync(endpoint, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<GetIngredientsResponse>(cancellationToken);
        }

        return GetIngredientsResponse.Empty;
    }

    public async Task<CreateRecipeResponse> CreateRecipe(CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(Constants.RecipesRoute, createRecipeRequest, options: null,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CreateRecipeResponse>(cancellationToken);
        }

        throw new Exception("Unable to create recipe");   // TODO: concrete types?
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id);
        var result = await httpClient.DeleteAsync(endpoint, cancellationToken);
        return result.IsSuccessStatusCode;
    }
}