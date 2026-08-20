namespace MealPlanner.Services.Menus.Exceptions;

public class MenuAlreadyExistsException(DateOnly date) : Exception
{
    public DateOnly Date { get; } = date;

}