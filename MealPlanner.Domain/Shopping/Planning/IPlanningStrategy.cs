namespace MealPlanner.Domain.Shopping.Planning;

public interface IPlanningStrategy
{
    // Precondition: allowedTrips is not empty. Returns one of allowedTrips.
    DateOnly Choose(IReadOnlyCollection<DateOnly> allowedTrips, IReadOnlySet<DateOnly> usedTrips);
}