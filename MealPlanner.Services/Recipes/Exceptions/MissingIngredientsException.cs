namespace MealPlanner.Services.Recipes.Exceptions;

public class MissingIngredientsException(IEnumerable<int> missingIds) : Exception
{
    public IEnumerable<int> Ids { get; } = missingIds;
}