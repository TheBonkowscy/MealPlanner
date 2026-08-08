namespace MealPlanner.Shared.Meals;

public record GetRecipesResponse(IEnumerable<MealListItemResponse> Meals)
{
    public static GetRecipesResponse Empty => new([]);
}