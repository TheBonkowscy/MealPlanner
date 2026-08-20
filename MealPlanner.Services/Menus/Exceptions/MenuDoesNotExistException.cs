namespace MealPlanner.Services.Menus.Exceptions;

public class MenuDoesNotExistException(DateOnly date) : Exception
{
    public DateOnly Date { get; } = date;
}