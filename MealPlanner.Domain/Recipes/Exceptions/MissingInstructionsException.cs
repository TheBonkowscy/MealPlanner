namespace MealPlanner.Domain.Recipes.Exceptions;

public class MissingInstructionsException : Exception
{
    public static void ThrowIfInstructionsAreInvalid(string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingInstructionsException();
}