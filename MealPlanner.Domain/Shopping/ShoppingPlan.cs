namespace MealPlanner.Domain.Shopping.Planning;

public record ShoppingPlan(int WeekNumber, int Year, IReadOnlyDictionary<DateOnly, List<ShoppingNeed>> Plan);