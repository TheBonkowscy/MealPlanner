using System.Globalization;

namespace MealPlanner.UI.Models;

public sealed record WeekMeal(DateOnly Date, int RecipeId, string Name, int Servings) : WeekDragItem
{
    public override string ZoneIdentifier => Date.ToString("O", CultureInfo.InvariantCulture);
}
