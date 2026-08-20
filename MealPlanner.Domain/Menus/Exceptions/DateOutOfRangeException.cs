namespace MealPlanner.Domain.Menus.Exceptions;

public class DateOutOfRangeException : Exception
{
    public Cause UnderlyingCause { get; }

    public DateOutOfRangeException(Cause cause)
    {
        UnderlyingCause = cause;
    }

    public static void ThrowIfNotInRange(DateOnly date)
    {
     
        DateOnly[] invalidDates = [DateOnly.MinValue, DateOnly.MaxValue];
        if (invalidDates.Contains(date))
        {
            Throw(Cause.Unset);
        }
        
        var dateTooFarInThePast = date < Menu.MinDateInThePast;
        if (dateTooFarInThePast)
        {
            Throw(Cause.TooFarInThePast);
        }
        
        var dateTooFarInTheFuture = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100) < date;
        if (dateTooFarInTheFuture)
        {
            Throw(Cause.TooFarInTheFuture);
        }
    }
    
    private static void Throw(Cause cause) => throw new DateOutOfRangeException(cause);

    public enum Cause
    {
        Unset,
        TooFarInThePast,
        TooFarInTheFuture
    }
}