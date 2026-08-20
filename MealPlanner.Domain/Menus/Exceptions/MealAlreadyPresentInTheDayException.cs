namespace MealPlanner.Domain.Menus.Exceptions;

public class MealAlreadyPresentInTheDayException(string name, DateOnly date) : Exception
{
    public string Name { get; } = name;
    public DateOnly Date { get; } = date;
}