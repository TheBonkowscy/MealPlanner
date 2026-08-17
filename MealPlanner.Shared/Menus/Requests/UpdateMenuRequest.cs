namespace MealPlanner.Shared.Menus.Requests;

public record UpdateMenuRequest(DateOnly Date, List<AddMealRequest> Meals);