namespace MealPlanner.Domain.Recipes.Exceptions;

public class MissingRecipeException : Exception
{
    public static void ThrowIfRecipeIsNull(Recipe? recipe)
    { 
        if (recipe is null)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingRecipeException();
}