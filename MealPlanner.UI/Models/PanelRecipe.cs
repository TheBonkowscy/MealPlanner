namespace MealPlanner.UI.Models;

public sealed record PanelRecipe(int RecipeId, string Name, int Servings) : WeekDragItem
{
    public const string Zone = "recipes";

    public override string ZoneIdentifier => Zone;
}
