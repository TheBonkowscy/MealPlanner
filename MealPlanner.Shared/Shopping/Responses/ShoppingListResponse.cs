namespace MealPlanner.Shared.Shopping.Responses;

public record ShoppingListResponse(DateOnly StartDate, PlanningStrategyKind Strategy, IReadOnlyList<PlannedTripResponse> Trips);