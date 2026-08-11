namespace MealPlanner.Shared.Recipes;

public record GetRecipesResponse(IEnumerable<RecipeListItemResponse> Recipes)
{
    public static GetRecipesResponse Empty => new([]);
}