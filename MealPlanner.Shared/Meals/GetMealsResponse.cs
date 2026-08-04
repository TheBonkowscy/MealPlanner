namespace MealPlanner.Shared.Meals;

public record GetMealsResponse(IEnumerable<MealListItemResponse> Meals)
{
    public static GetMealsResponse Empty => new([]);
}