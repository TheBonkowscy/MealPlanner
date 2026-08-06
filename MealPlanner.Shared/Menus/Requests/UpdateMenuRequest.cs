namespace MealPlanner.Shared.Menus.Requests;

public record UpdateMenuRequest(DateOnly Date, Dictionary<int, string> Meals);