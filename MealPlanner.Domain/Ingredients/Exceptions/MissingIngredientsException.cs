using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Ingredients.Exceptions;

public class MissingIngredientsException : Exception
{
    public static void ThrowIfIngredientsMissing(ICollection<AddIngredientAction> ingredients)
    {
        if (ingredients.Count == 0)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingIngredientsException();
}