namespace MealPlanner.Shared.Ingredients;

public record GetIngredientsResponse(IEnumerable<IngredientListItemResponse> Ingredients);