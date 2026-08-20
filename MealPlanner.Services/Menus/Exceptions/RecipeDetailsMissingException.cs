namespace MealPlanner.Services.Menus.Exceptions;

public class RecipeDetailsMissingException(string name) : Exception
{
    public string Name { get; } = name;
    
    public static void Throw(string name) => throw new RecipeDetailsMissingException(name);
}