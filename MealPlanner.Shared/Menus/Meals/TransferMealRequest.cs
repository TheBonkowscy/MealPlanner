namespace MealPlanner.Shared.Menus.Meals;

public record TransferMealRequest(DateOnly NewDate, int Order, TransferMode TransferMode);