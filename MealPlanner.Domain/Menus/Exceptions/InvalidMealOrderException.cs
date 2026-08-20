namespace MealPlanner.Domain.Menus.Exceptions;

public class InvalidMealOrderException : Exception
{
    public Cause UnderlyingCause { get; }

    private InvalidMealOrderException(Cause cause)
    {
        UnderlyingCause = cause;
    }
    
    public static void ThrowIfOrderIsInvalid(int order)
    {
        if (order < Menu.MinOrder)
        {
            Throw(Cause.OrderInvalid);
        }
    }
    
    private static void Throw(Cause cause) => throw new InvalidMealOrderException(cause);

    public static void ThrowIfExceedsNumberOfMeals(int order, int mealsCount)
    {
        if (order > mealsCount +1 && mealsCount != 0)
        {
            Throw(Cause.ExceedsRange);
        }
    }

    public enum Cause
    {
        OrderInvalid,
        ExceedsRange
    }
}