namespace MealPlanner.UI.Services;

public abstract record WeekDropAction
{
    public static WeekDropAction None { get; } = new NoAction();

    public sealed record ReorderMeal(DateOnly Day, int RecipeId, int Position) : WeekDropAction;

    private sealed record NoAction : WeekDropAction;
}
