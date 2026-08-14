using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

public interface IFindRecipes
{
    Task<GetRecipesResponse> Get(string? query, CancellationToken cancellationToken = default);
    Task<GetRecipeDetailsResponse?> Get(int id, CancellationToken cancellationToken = default);
}