using MealPlanner.Domain.Recipes;

namespace MealPlanner.Domain.Menus.Exceptions;

public class MealExistsAtPositionException : Exception
{
    private int Order { get; }
    
    private MealExistsAtPositionException(int order)
    {
        Order = order;
    }
    
    public static void ThrowIfExists(Recipe? recipe, int order)
    {
        if (recipe is not null)
        {
            Throw(order);
        }
    }

    private static void Throw(int order) => throw new MealExistsAtPositionException(order);
}