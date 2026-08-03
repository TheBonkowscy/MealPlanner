namespace MealPlanner.Shared.Meals;

public record GetMealsResponse(IEnumerable<MealListItemResponse> Meals);