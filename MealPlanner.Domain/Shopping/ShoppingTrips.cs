using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.Shopping;

public record ShoppingTrips(DateOnly StartDate, IReadOnlyList<PlannedTrip> Trips);

public record PlannedTrip(DateOnly Date, IReadOnlyList<PlannedItem> Items);

public record PlannedItem(int IngredientId, string IngredientName, MeasureUnit Unit, decimal Quantity, bool ShelfLifeWarning);