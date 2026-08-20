namespace MealPlanner.Domain.Recipes.Exceptions;

public class MissingRecipeNameException : Exception
{
    public static void ThrowIfNameIsInvalid(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingRecipeNameException();
}