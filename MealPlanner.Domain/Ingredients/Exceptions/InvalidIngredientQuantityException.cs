namespace MealPlanner.Domain.Ingredients.Exceptions;

public class InvalidIngredientQuantityException : Exception
{
    public static void ThrowIfQuantityIsInvalid(decimal quantity)
    {
        
    }
}