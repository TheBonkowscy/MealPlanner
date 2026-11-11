using AwesomeAssertions;
using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Services.Shopping;
using MealPlanner.Shared.Shopping;

namespace MealPlanner.Services.Tests.Shopping;

public class PlanningStrategyFactoryTests
{
    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, typeof(ReuseExistingTripStrategy))]
    [InlineData(PlanningStrategyKind.LatestInWindow, typeof(LatestInWindowStrategy))]
    public void CreatePlanningStrategy_ReturnsTheStrategyForTheKind(PlanningStrategyKind kind, Type expected)
    {
        // Act
        var strategy = PlanningStrategyFactory.CreatePlanningStrategy(kind);

        // Assert
        strategy.Should().BeOfType(expected);
    }

    [Fact]
    public void CreatePlanningStrategy_MapsEveryDefinedKindToItsOwnStrategy()
    {
        // Arrange: a kind added to the enum without a mapping would end up as an exception at the first request
        var kinds = Enum.GetValues<PlanningStrategyKind>();

        // Act
        var strategies = kinds.Select(PlanningStrategyFactory.CreatePlanningStrategy).ToList();

        // Assert
        strategies.Select(strategy => strategy.GetType()).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void CreatePlanningStrategy_Throws_WhenTheKindIsNotDefined()
    {
        // Arrange: a value that arrives from outside, such as an out-of-range number in the query string
        const PlanningStrategyKind undefined = (PlanningStrategyKind)99;

        // Act
        var act = () => PlanningStrategyFactory.CreatePlanningStrategy(undefined);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("strategy");
    }
}
