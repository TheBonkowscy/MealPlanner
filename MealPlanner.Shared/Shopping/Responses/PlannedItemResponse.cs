using MealPlanner.Shared.Shared;

namespace MealPlanner.Shared.Shopping.Responses;

public record PlannedItemResponse(string IngredientName, MeasureUnitDto Unit, decimal Quantity, bool ShelfLifeWarning);