namespace MealPlanner.Shared.Ingredients;

public record IngredientListItemResponse(int Id, string Name, IEnumerable<IngredientMeasureUnitsResponse> Units);