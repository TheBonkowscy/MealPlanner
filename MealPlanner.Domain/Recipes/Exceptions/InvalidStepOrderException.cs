namespace MealPlanner.Domain.Recipes.Exceptions;

public class InvalidStepOrderException : Exception
{
    public static void ThrowIfOrderIsInvalid(int order)
    {
        if (order < 1)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new InvalidStepOrderException();
}