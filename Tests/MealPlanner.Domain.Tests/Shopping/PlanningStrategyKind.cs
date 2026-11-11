using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Shared.Shopping;

namespace MealPlanner.Domain.Tests.Shopping;

public static class PlanningStrategyKindExtensions
{
    public static IPlanningStrategy Create(this PlanningStrategyKind kind) => kind switch
    {
        PlanningStrategyKind.ReuseExistingTrip => new ReuseExistingTripStrategy(),
        PlanningStrategyKind.LatestInWindow => new LatestInWindowStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
