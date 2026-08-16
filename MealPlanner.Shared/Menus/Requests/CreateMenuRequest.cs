namespace MealPlanner.Shared.Menus.Requests;

public record CreateMenuRequest(DateOnly Date, List<AddMealRequest> Meals);

public record AddMealRequest(int Id, int Order, int Servings);