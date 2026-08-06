namespace MealPlanner.Shared.Menus.Requests;

public record CreateMenuRequest(DateOnly Date, Dictionary<int, string> Meals);