using MealPlanner.Domain.Shopping.Planning;

namespace MealPlanner.Domain.Tests.Shopping;

// Lets [Theory] rows name a strategy: the strategy classes themselves are not serializable test data.
public enum PlanningStrategyKind
{
    ReuseExistingTrip,
    LatestInWindow
}

public static class PlanningStrategyKindExtensions
{
    public static IPlanningStrategy Create(this PlanningStrategyKind kind) => kind switch
    {
        PlanningStrategyKind.ReuseExistingTrip => new ReuseExistingTripStrategy(),
        PlanningStrategyKind.LatestInWindow => new LatestInWindowStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
