namespace MealPlanner.Domain.Menus.Exceptions;

public class InvalidNumberOfMealServingsException : Exception
{
    public static void ThrowIfServingsIsInvalid(int servings)
    {
        if (servings < 1)
        {
            Throw();
        }
    }

    private static void Throw() => throw new InvalidNumberOfMealServingsException();
}