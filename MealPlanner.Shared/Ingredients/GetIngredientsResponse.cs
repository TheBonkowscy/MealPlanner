namespace MealPlanner.Shared.Ingredients;

public record GetIngredientsResponse(IEnumerable<IngredientListItemResponse> Ingredients)
{
    public static GetIngredientsResponse Empty => new([]);
}