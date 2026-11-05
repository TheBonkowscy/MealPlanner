namespace MealPlanner.Domain.Shopping.Planning;

public class LatestInWindowStrategy : IPlanningStrategy
{
    public DateOnly Choose(IReadOnlyCollection<DateOnly> allowedTrips, IReadOnlySet<DateOnly> usedTrips) =>
        allowedTrips.Max();
}