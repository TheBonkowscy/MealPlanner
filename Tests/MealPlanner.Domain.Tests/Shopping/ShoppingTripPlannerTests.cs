using System.Globalization;
using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Shopping;
using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests.Shopping;

public class ShoppingTripPlannerTests
{
    // Fridays: 2026-10-23 and 2026-12-25 (Christmas Day, a closed day).
    private static readonly DateOnly BigShopFriday = new(2026, 10, 23);
    private static readonly DateOnly ChristmasFriday = new(2026, 12, 25);

    private readonly IPlanShoppingTrips _planner = new ShoppingTripPlanner();

    private static ShoppingNeed Need(string name, int? shelfLifeDays, DateOnly day, decimal quantity = 1,
        MeasureUnit unit = MeasureUnit.Piece) => TestShoppingNeeds.Create(name, day, shelfLifeDays, quantity, unit);

    private ShoppingTrips Plan(PlanningStrategyKind strategy, DateOnly start, List<ShoppingNeed> needs,
        params DateOnly[] closedDays)
    {
        var result = _planner.PlanTrips(start, needs, closedDays.ToHashSet(), strategy.Create());

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    // "23[Pasta=1];24[Tomato=2!]": trip day, then name=quantity per item; "!" marks a shelf-life warning.
    private static string Summarize(ShoppingTrips plan) => string.Join(";", plan.Trips.Select(trip =>
        $"{trip.Date.Day}[{string.Join(",", trip.Items.Select(Describe))}]"));

    private static string Describe(PlannedItem item) =>
        $"{item.IngredientName}={item.Quantity.ToString(CultureInfo.InvariantCulture)}{(item.ShelfLifeWarning ? "!" : "")}";

    // --- Choosing a trip for a need ---

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "23[Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "23[Pasta=1]")]
    public void PlanTrips_DurableNeed_GoesToBigTrip(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(28))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "28[Cake=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "28[Cake=1]")]
    public void PlanTrips_ZeroShelfLifeNeed_UsesSameDayTrip(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Cake", 0, TestDates.Oct(28))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "25[Cake=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "25[Cake=1]")]
    public void PlanTrips_ZeroShelfLifeNeedOnSunday_CreatesSameDayTripOnSunday(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Cake", 0, TestDates.Oct(25))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "23[Pasta=1,Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "23[Pasta=1];24[Tomato=1]")]
    public void PlanTrips_SundayNeed_PrefersFridayWhenOnlyFridayIsUsed(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(24)), Need("Tomato", 3, TestDates.Oct(25))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Tomato=5]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Tomato=2];28[Tomato=3]")]
    public void PlanTrips_TwoNeedsOfSameIngredient_ShareTripOnlyWhenReusing(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 4, TestDates.Oct(28), 2), Need("Tomato", 4, TestDates.Oct(29), 3)];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "24[Bread=1!]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "24[Bread=1!]")]
    public void PlanTrips_EmptyWindow_FallsBackToLatestTripWithWarning(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Bread", 1, TestDates.Oct(26))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Tomato=1]")]
    public void PlanTrips_ClosedFreshDay_IsSkipped(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 4, TestDates.Oct(28))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs, TestDates.Oct(27));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Cake=1!]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Cake=1!]")]
    public void PlanTrips_ClosedSameDay_FallsBackWithWarning(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Cake", 0, TestDates.Oct(28))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs, TestDates.Oct(28));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "23[Pasta=1,Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "23[Pasta=1];27[Tomato=1]")]
    public void PlanTrips_DurableNeeds_AreAssignedBeforeOtherNeeds(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 5, TestDates.Oct(28)), Need("Pasta", null, TestDates.Oct(29))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Tomato=1]")]
    public void PlanTrips_WithoutDurableNeeds_DoesNotTreatBigTripAsUsed(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 5, TestDates.Oct(28))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "24[Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "24[Pasta=1]")]
    public void PlanTrips_ClosedFriday_WhenThursdayAndSaturdayAreOpen_EarlierDayWins(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, ChristmasFriday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Bread=1,Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Bread=1,Pasta=1]")]
    public void PlanTrips_ClosedFridayAndThursday_MoveBigTripToSaturday(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(26)), Need("Bread", 1, TestDates.Dec(26))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Pasta=1]")]
    public void PlanTrips_NearerLaterDay_BeatsFurtherEarlierDay(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "24[Bread=1!,Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "24[Bread=1!,Pasta=1]")]
    public void PlanTrips_ClosedFridayAndSaturday_MoveBigTripToThursday(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(28)), Need("Bread", 1, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "23[Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "23[Pasta=1]")]
    public void PlanTrips_ClosedThursdayFridayAndSaturday_MoveBigTripToWednesday(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Pasta=1];28[Bread=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Pasta=1];28[Bread=1]")]
    public void PlanTrips_WhenEarlierDaysAreClosed_BigTripMovesForwardToSunday(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(28)), Need("Bread", 1, TestDates.Dec(29))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(23), TestDates.Dec(24), ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Tomato=1]")]
    public void PlanTrips_NeedOnForwardBigTripDay_IsBoughtThatDayWithoutWarning(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 2, TestDates.Dec(27))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(23), TestDates.Dec(24), ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Tomato=1!]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Tomato=1!]")]
    public void PlanTrips_NeedDatedBeforeForwardBigTrip_IsBoughtOnBigTripWithWarning(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 2, TestDates.Dec(26))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(23), TestDates.Dec(24), ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "27[Pasta=1!,Rice=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "27[Pasta=1!,Rice=1]")]
    public void PlanTrips_DurableNeedDatedBeforeForwardBigTrip_IsFlagged(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(26)), Need("Rice", null, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(23), TestDates.Dec(24), ChristmasFriday, TestDates.Dec(26));

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_BigTripSixDaysBeforeTheFriday_IsStillFound(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(25))];
        var closedDays = Enumerable.Range(18, 12).Select(TestDates.Oct).ToArray();

        // Act
        var plan = Plan(strategy, BigShopFriday, needs, closedDays);

        // Assert
        Summarize(plan).Should().Be("17[Pasta=1]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_BigTripSixDaysAfterTheFriday_IsStillFound(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(30))];
        var closedDays = Enumerable.Range(17, 12).Select(TestDates.Oct).ToArray();

        // Act
        var plan = Plan(strategy, BigShopFriday, needs, closedDays);

        // Assert
        Summarize(plan).Should().Be("29[Pasta=1]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Tomato=1]")]
    public void PlanTrips_NeedOnBigTripSaturday_IsBoughtThatDayWithoutWarning(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 2, TestDates.Dec(26))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Bread=1!]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Bread=1!]")]
    public void PlanTrips_NeedAfterBigTripSaturday_FallsBackOverSundayWithWarning(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Bread", 1, TestDates.Dec(28))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "26[Cake=1,Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "26[Cake=1,Pasta=1]")]
    public void PlanTrips_BigTripAndFreshTripOnSaturday_AreOneTrip(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Dec(26)), Need("Cake", 0, TestDates.Dec(26))];

        // Act
        var plan = Plan(strategy, ChristmasFriday, needs, TestDates.Dec(24), ChristmasFriday);

        // Assert
        plan.Trips.Should().ContainSingle();
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_WhenEveryEarlierDayIsClosed_UsesSaturdayForBigTrip(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(25))];
        var closedWeek = Enumerable.Range(17, 7).Select(TestDates.Oct).ToArray();

        // Act
        var plan = Plan(strategy, BigShopFriday, needs, closedWeek);

        // Assert
        Summarize(plan).Should().Be("24[Pasta=1]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_SameIngredientAndUnitInOneTrip_SumsQuantities(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs =
        [
            Need("Pasta", null, TestDates.Oct(24), 200, MeasureUnit.Gram),
            Need("Pasta", null, TestDates.Oct(25), 300, MeasureUnit.Gram)
        ];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be("23[Pasta=500]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_SameIngredientWithDifferentUnits_KeepsSeparateItems(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs =
        [
            Need("Milk", null, TestDates.Oct(24), 200, MeasureUnit.Milliliter),
            Need("Milk", null, TestDates.Oct(24), 1, MeasureUnit.GlassCup)
        ];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        var items = plan.Trips.Should().ContainSingle().Which.Items;
        items.Should().HaveCount(2);
        items.Select(item => item.Unit).Should().BeEquivalentTo([MeasureUnit.Milliliter, MeasureUnit.GlassCup]);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_WhenAnyContributingNeedHasWarning_FlagsTheItem(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Bread", 1, TestDates.Oct(26)), Need("Bread", 3, TestDates.Oct(26))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be("24[Bread=2!]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_WithoutNeeds_ReturnsNoTrips(PlanningStrategyKind strategy)
    {
        // Act
        var plan = Plan(strategy, BigShopFriday, []);

        // Assert
        plan.StartDate.Should().Be(BigShopFriday);
        plan.Trips.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_NeedsOutsideTheCycle_AreIgnored(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(23)), Need("Pasta", null, TestDates.Oct(31))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        plan.Trips.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_HugeShelfLife_BehavesLikeLongShelfLifeWithoutThrowing(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Milk", int.MaxValue, TestDates.Oct(24))];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        Summarize(plan).Should().Be("23[Milk=1]");
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_ReturnsTripsInAscendingDateOrder(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs =
        [
            Need("Tomato", 4, TestDates.Oct(29), 3),
            Need("Cake", 0, TestDates.Oct(25)),
            Need("Pasta", null, TestDates.Oct(30))
        ];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);

        // Assert
        plan.Trips.Select(trip => trip.Date).Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_GivesTheSamePlanRegardlessOfInputOrder(PlanningStrategyKind strategy)
    {
        // Arrange
        List<ShoppingNeed> needs = [Need("Tomato", 4, TestDates.Oct(28), 2), Need("Tomato", 4, TestDates.Oct(29), 3), Need("Cake", 0, TestDates.Oct(28))];
        List<ShoppingNeed> reversed = [.. needs.AsEnumerable().Reverse()];

        // Act
        var plan = Plan(strategy, BigShopFriday, needs);
        var planFromReversed = Plan(strategy, BigShopFriday, reversed);

        // Assert
        planFromReversed.Should().BeEquivalentTo(plan, options => options.WithStrictOrdering());
    }

    public static TheoryData<PlanningStrategyKind, int> EveryStartWeekday()
    {
        // TestDates.Oct 19 (Monday) ... TestDates.Oct 25 (Sunday) 2026
        var data = new TheoryData<PlanningStrategyKind, int>();
        foreach (var strategy in Enum.GetValues<PlanningStrategyKind>())
        {
            foreach (var day in Enumerable.Range(19, 7))
            {
                data.Add(strategy, day);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryStartWeekday))]
    public void PlanTrips_StartOnAnyWeekday_UsesTheStartDayAsBigTrip(PlanningStrategyKind strategy, int dayOfOctober)
    {
        // Arrange
        var start = TestDates.Oct(dayOfOctober);
        List<ShoppingNeed> needs = [Need("Pasta", null, start.AddDays(3))];

        // Act
        var plan = Plan(strategy, start, needs);

        // Assert
        plan.Trips.Should().ContainSingle();
        plan.Trips[0].Date.Should().Be(start);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "24[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "24[Tomato=1]")]
    public void PlanTrips_StartOnWednesday_PlansFreshTripsOnShopDays(PlanningStrategyKind strategy, string expected)
    {
        // Arrange
        var wednesday = TestDates.Oct(21);
        List<ShoppingNeed> needs = [Need("Tomato", 2, TestDates.Oct(26))];

        // Act
        var plan = Plan(strategy, wednesday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "25[Tomato=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "25[Tomato=1]")]
    public void PlanTrips_StartOnSunday_BigTripIsATripForTheNextDayNeed(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        var sunday = TestDates.Oct(25);
        List<ShoppingNeed> needs = [Need("Tomato", 1, TestDates.Oct(26))];

        // Act
        var plan = Plan(strategy, sunday, needs);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip, "20[Pasta=1]")]
    [InlineData(PlanningStrategyKind.LatestInWindow, "20[Pasta=1]")]
    public void PlanTrips_ClosedStartDayOnWednesday_MovesBigTripToTheEarlierDay(PlanningStrategyKind strategy,
        string expected)
    {
        // Arrange
        var wednesday = TestDates.Oct(21);
        List<ShoppingNeed> needs = [Need("Pasta", null, TestDates.Oct(24))];

        // Act
        var plan = Plan(strategy, wednesday, needs, wednesday);

        // Assert
        Summarize(plan).Should().Be(expected);
    }

    // --- Errors ---

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public void PlanTrips_WhenNoDayIsOpenForTheBigTrip_Fails(PlanningStrategyKind strategy)
    {
        // Arrange
        var closedDays = Enumerable.Range(17, 13).Select(TestDates.Oct).ToHashSet();

        // Act
        var result = _planner.PlanTrips(BigShopFriday, [], closedDays, strategy.Create());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Shopping.NoOpenBigShopDay(BigShopFriday));
    }
}
