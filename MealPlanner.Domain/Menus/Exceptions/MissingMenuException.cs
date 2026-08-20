namespace MealPlanner.Domain.Menus.Exceptions;

public class MissingMenuException : Exception
{
    public static void ThrowIfMenuIsNull(Menu? menu)
    {
        if (menu is null)
        {
            Throw();
        }
    }

    private static void Throw() => throw new MissingMenuException();
}