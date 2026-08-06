namespace MealPlanner.Shared.Menus.Responses;

public record GetMenuResponse(int Id, DateOnly Date, Dictionary<int, string> Meals);