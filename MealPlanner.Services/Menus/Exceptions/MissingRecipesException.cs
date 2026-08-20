namespace MealPlanner.Services.Menus.Exceptions;

public class MissingRecipesException(IEnumerable<int> missingIds) : Exception
{
    public IEnumerable<int> Ids { get; } = missingIds;
}