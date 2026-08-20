namespace MealPlanner.Domain.Recipes.Exceptions;

public class InvalidNumberOfServingsException : Exception
{
    public static void ThrowIfServingsIsInvalid(int servings)
    {
        if (servings < 1)
        {
            Throw();
        }
    }

    private static void Throw() => throw new InvalidNumberOfServingsException();
}