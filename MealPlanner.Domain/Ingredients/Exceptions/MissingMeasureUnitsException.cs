namespace MealPlanner.Domain.Ingredients.Exceptions;

public class MissingMeasureUnitsException : Exception
{
    public static void ThrowIfEmpty(ICollection<MeasureUnit> units)
    {
        if (units.Count == 0)
        {
            Throw();
        }
    }

    private static void Throw() => throw new MissingMeasureUnitsException();
}