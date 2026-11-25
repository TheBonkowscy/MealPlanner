namespace MealPlanner.Shared.Menus.Requests;

public record CreateMealRequest(int RecipeId, int Order, int Servings);