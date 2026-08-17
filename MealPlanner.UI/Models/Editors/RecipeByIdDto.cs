namespace MealPlanner.UI.Models.Editors;

public record RecipeByIdDto(int Id, string Name)
{
    public override string ToString() => Name;
}