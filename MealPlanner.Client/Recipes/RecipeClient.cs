using System.Net.Http.Json;
using Flurl;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

internal class RecipeClient(HttpClient httpClient) : 
    IFindRecipes, ICreateRecipes, IDeleteRecipes, IUpdateRecipes,
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

    public async Task<GetRecipeDetailsResponse> UpdateRecipe(int id, UpdateRecipeRequest updateRecipeRequest, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id);
        var response =
            await httpClient.PutAsJsonAsync(endpoint, updateRecipeRequest, options: null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetRecipeDetailsResponse>(cancellationToken);
        }

        throw new Exception("Unable to update recipe");   // TODO: concrete types?
    }

    public async Task<GetRecipeDetailsResponse> AddIngredientToRecipe(int id, UpdateRecipeIngredientRequest request, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id).AppendPathSegment("/ingredients/").AppendPathSegment(request.Id);
        var response =
            await httpClient.PutAsJsonAsync(endpoint, request, options: null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetRecipeDetailsResponse>(cancellationToken);
        }

        throw new Exception("Unable to update ingredients for this recipe");   // TODO: concrete types?
    }

    public async Task DeleteIngredientFromRecipe(int id, DeleteRecipeIngredientRequest deleteRecipeIngredientRequest,
        CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id).AppendPathSegment("/ingredients/").AppendPathSegment(deleteRecipeIngredientRequest.Id);
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
        {
            Content = JsonContent.Create(deleteRecipeIngredientRequest)
        };
        
        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new Exception("Unable to delete ingredients from this recipe");   // TODO: concrete types?
    }

    public async Task<GetRecipeDetailsResponse> AddStep(int id, AddRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id).AppendPathSegment("/steps/");
        var response = await httpClient.PostAsJsonAsync(endpoint, request, options: null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetRecipeDetailsResponse>(cancellationToken);
        }

        throw new Exception("Unable to add recipe step");   // TODO: concrete types?
    }

    public async Task<GetRecipeDetailsResponse> UpdateStep(int id, UpdateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id).AppendPathSegment("/steps/").AppendPathSegment(request.Id);
        var response = await httpClient.PutAsJsonAsync(endpoint, request, options: null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetRecipeDetailsResponse>(cancellationToken);
        }

        throw new Exception("Unable to update recipe step");   // TODO: concrete types?
    }

    public async Task DeleteStep(int id, int stepId, CancellationToken cancellationToken)
    {
        var endpoint = Constants.RecipesRoute.AppendPathSegment(id).AppendPathSegment("/ingredients/").AppendPathSegment(stepId);
        var response = await httpClient.DeleteAsync(endpoint, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new Exception("Unable to delete ingredients from this recipe");   // TODO: concrete types?
    }
}