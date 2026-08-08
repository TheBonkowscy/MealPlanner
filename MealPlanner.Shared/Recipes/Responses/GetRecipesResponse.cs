namespace MealPlanner.Shared.Recipes.Responses;

public record GetRecipesResponse(IEnumerable<RecipeListItemResponse> Recipes)
{
    public static GetRecipesResponse Empty => new([]);
}