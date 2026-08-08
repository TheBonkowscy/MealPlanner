namespace MealPlanner.Shared.Meals;

public record GetRecipesResponse(IEnumerable<RecipeListItemResponse> Recipes)
{
    public static GetRecipesResponse Empty => new([]);
}