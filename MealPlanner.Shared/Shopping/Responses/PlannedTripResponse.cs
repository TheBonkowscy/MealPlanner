namespace MealPlanner.Shared.Shopping.Responses;

public record PlannedTripResponse(DateOnly Date, IReadOnlyList<PlannedItemResponse> Items);