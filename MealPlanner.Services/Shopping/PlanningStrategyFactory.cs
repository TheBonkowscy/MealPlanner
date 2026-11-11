using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Shared.Shopping;

namespace MealPlanner.Services.Shopping;

public static class PlanningStrategyFactory
{
    public static IPlanningStrategy CreatePlanningStrategy(PlanningStrategyKind strategy)
    {
        return strategy switch
        {
            PlanningStrategyKind.LatestInWindow => new LatestInWindowStrategy(),
            PlanningStrategyKind.ReuseExistingTrip => new ReuseExistingTripStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };
    }
}