using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Ingredients.Exceptions;

public class InvalidIngredientQuantityException : Exception
{
    public static void ThrowIfQuantityIsInvalid(ICollection<AddIngredientAction> ingredients)
    {
        foreach (var addIngredientAction in ingredients)
        {
            ThrowIfQuantityIsInvalid(addIngredientAction.Quantity);
        }
    }

    public static void ThrowIfQuantityIsInvalid(decimal quantity)
    {
        if (quantity <= 0)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new InvalidIngredientQuantityException();
}