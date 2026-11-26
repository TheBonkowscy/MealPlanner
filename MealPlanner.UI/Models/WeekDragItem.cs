namespace MealPlanner.UI.Models;

public abstract record WeekDragItem : IReorderable
{
    public abstract string ZoneIdentifier { get; }
}
