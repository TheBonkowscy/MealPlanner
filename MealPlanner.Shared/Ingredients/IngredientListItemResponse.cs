using MealPlanner.Shared.Shared;

namespace MealPlanner.Shared.Ingredients;

public record IngredientListItemResponse(int Id, string Name, IEnumerable<MeasureUnitDto> ApplicableUnits);