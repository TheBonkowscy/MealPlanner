namespace MealPlanner.Domain.Ingredients.Exceptions;

public class MissingIngredientNameException : Exception
{
    public static void ThrowIfNameIsInvalid(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingIngredientNameException();
}