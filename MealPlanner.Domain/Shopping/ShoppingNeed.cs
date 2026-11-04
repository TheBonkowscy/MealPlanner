using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.Shopping;

public record ShoppingNeed(
    int IngredientId,
    string IngredientName,
    decimal Quantity,
    MeasureUnit Unit,
    int? ShelfLifeDays,
    DateOnly ConsumptionDate);