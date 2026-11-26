namespace MealPlanner.UI.Services;

public static class WeekDropDecision
{
    public static WeekDropAction Decide(
        DateOnly sourceDay,
        int recipeId,
        DateOnly targetDay,
        int dropIndex,
        IReadOnlyList<int> recipeIdsInDay)
    {
        if (sourceDay != targetDay)
        {
            return WeekDropAction.None;
        }

        var currentPosition = CurrentPositionOf(recipeId, recipeIdsInDay);
        if (currentPosition is null)
        {
            return WeekDropAction.None;
        }

        var position = Math.Clamp(dropIndex + 1, 1, recipeIdsInDay.Count);

        return position == currentPosition
            ? WeekDropAction.None
            : new WeekDropAction.ReorderMeal(targetDay, recipeId, position);
    }

    private static int? CurrentPositionOf(int recipeId, IReadOnlyList<int> recipeIdsInDay)
    {
        for (var index = 0; index < recipeIdsInDay.Count; index++)
        {
            if (recipeIdsInDay[index] == recipeId)
            {
                return index + 1;
            }
        }

        return null;
    }
}
