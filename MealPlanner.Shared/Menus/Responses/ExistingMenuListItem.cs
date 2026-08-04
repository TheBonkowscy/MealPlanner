namespace MealPlanner.Shared.Menus.Responses;

public record ExistingMenuListItem(int Id, DateOnly Day, bool HasMeals);