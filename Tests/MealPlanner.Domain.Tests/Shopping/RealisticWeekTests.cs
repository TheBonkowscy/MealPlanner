using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shopping;
using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests.Shopping;

// Checks properties that must hold for any sensible week instead of single scenarios.
// Every need has a distinct ingredient name, so each planned item maps to exactly one need.
public class RealisticWeekTests
{
    private static readonly DateOnly BigShopFriday = new(2026, 10, 23);

    private readonly IPlanShoppingTrips _planner = new ShoppingTripPlanner();

    private static DateOnly Oct(int day) => new(2026, 10, day);

    private static ShoppingNeed Need(string name, int? shelfLifeDays, DateOnly day) =>
        TestShoppingNeeds.Create(name, day, shelfLifeDays);

    private static List<ShoppingNeed> RealisticWeek() =>
    [
        Need("Makaron", null, Oct(24)),
        Need("Kiełbasa", 1, Oct(24)),
        Need("Bułki", 0, Oct(25)),
        Need("Szynka", 3, Oct(25)),
        Need("Banany", 1, Oct(25)),
        Need("Pieczywo", 1, Oct(26)),
        Need("Ryż", null, Oct(27)),
        Need("Sałata", 2, Oct(27)),
        Need("Pomidor", 4, Oct(28)),
        Need("Mleko", 2, Oct(28)),
        Need("Ogórek", 4, Oct(29)),
        Need("Jogurt", 2, Oct(29)),
        Need("Ser", null, Oct(30)),
        Need("Bułeczki", 0, Oct(30))
    ];

    private ShoppingTrips Plan(PlanningStrategyKind strategy, List<ShoppingNeed> needs)
    {
        var result = _planner.PlanTrips(BigShopFriday, needs, new HashSet<DateOnly>(), strategy.Create());

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_ForItemsWithoutWarning_RespectsShelfLifeWindows(PlanningStrategyKind strategy)
    {
        // Arrange
        var needs = RealisticWeek();

        // Act
        var plan = Plan(strategy, needs);

        // Assert
        var items = plan.Trips.SelectMany(trip => trip.Items.Select(item => (TripDate: trip.Date, Item: item))).ToList();
        items.Should().HaveCount(needs.Count);
        items.Should().Contain(x => x.Item.ShelfLifeWarning, "the fixture contains a need that cannot be satisfied in its window");

        foreach (var (tripDate, item) in items.Where(x => !x.Item.ShelfLifeWarning))
        {
            var need = needs.Single(n => n.IngredientName == item.IngredientName);
            var latestAllowed = need.ShelfLifeDays == 0 ? need.ConsumptionDate : need.ConsumptionDate.AddDays(-1);

            tripDate.Should().BeOnOrBefore(latestAllowed, "{0} must be bought in time", need.IngredientName);
            if (need.ShelfLifeDays is { } shelfLife)
            {
                tripDate.DayNumber.Should().BeGreaterThanOrEqualTo(need.ConsumptionDate.DayNumber - shelfLife,
                    "{0} must not be bought earlier than its shelf life allows", need.IngredientName);
            }
        }
    }

    // This holds for this fixture, not in general. If it ever fails, investigate instead of loosening the assertion.
    [Fact]
    public void PlanTrips_WithReuse_UsesNoMoreTripsThanLatestInWindow()
    {
        // Act
        var reuse = Plan(PlanningStrategyKind.ReuseExistingTrip, RealisticWeek());
        var latest = Plan(PlanningStrategyKind.LatestInWindow, RealisticWeek());

        // Assert
        reuse.Trips.Count.Should().BeLessThanOrEqualTo(latest.Trips.Count);
    }
}
