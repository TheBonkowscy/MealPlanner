namespace MealPlanner.Domain.Shopping.Planning;

public class ReuseExistingTripStrategy : IPlanningStrategy
{
    public DateOnly Choose(IReadOnlyCollection<DateOnly> allowedTrips, IReadOnlySet<DateOnly> usedTrips)
    {
        var reusable = allowedTrips.Where(usedTrips.Contains).ToList();
        return reusable.Count > 0 ? reusable.Max() : allowedTrips.Max();
    }
}