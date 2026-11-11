using System.Globalization;
using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Shopping;
using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Shopping;
using MealPlanner.Shared.Shared;
using MealPlanner.Shared.Shopping;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;

namespace MealPlanner.Services.Tests.Shopping;

public class ShoppingListGeneratorTests
{
    private static readonly DateOnly Friday = TestDates.Oct(2);
    private static readonly DateOnly Wednesday = TestDates.Oct(7);

    private readonly Mock<IShoppingNeedsSource> _needsSource = new();
    private readonly Mock<IClosedDaysProvider> _closedDaysProvider = new();
    private readonly ShoppingListGenerator _sut;

    public ShoppingListGeneratorTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        localizer.Setup(x => x["Gram"]).Returns(new LocalizedString("Gram", "g"));
        
        GivenNeeds();
        GivenClosedDays();

        // The real planner: it is pure domain code, so the plans below are the planner's own results
        _sut = new ShoppingListGenerator(_needsSource.Object, _closedDaysProvider.Object, new ShoppingTripPlanner(),
            new MeasureUnitMapper(localizer.Object));
    }

    [Theory]
    [MemberData(nameof(RangeCases))]
    public async Task Generate_ReadsNeedsForTheCycleAndClosedDaysAroundIt(
        DateOnly start, DateOnly needsFrom, DateOnly needsTo, DateOnly closedFrom, DateOnly closedTo)
    {
        // Act
        var result = await _sut.Generate(start, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        result.Value.Trips.Should().BeEmpty();
        _needsSource.Verify(x => x.GetShoppingNeeds(needsFrom, needsTo, It.IsAny<CancellationToken>()), Times.Once);
        _closedDaysProvider.Verify(x => x.GetClosedDays(closedFrom, closedTo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Generate_AcceptsAnyWeekdayAsTheBigShopDay()
    {
        // Arrange: Wednesday 7 October, and a durable need on Saturday 10 October
        GivenNeeds(TestShoppingNeeds.Create("Rice", TestDates.Oct(10), shelfLifeDays: null));

        // Act
        var result = await _sut.Generate(Wednesday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        result.Value.Trips.Should().ContainSingle().Which.Date.Should().Be(Wednesday);
    }

    [Theory]
    [MemberData(nameof(StrategyCases))]
    public async Task Generate_UsesTheRequestedStrategy(PlanningStrategyKind strategy, string[] expectedTrips)
    {
        // Arrange: bread for Saturday (shelf life 1) can only be bought on the Friday big trip;
        // tomatoes for Sunday (shelf life 2) can be bought on Friday or Saturday
        GivenNeeds(
            TestShoppingNeeds.Create("Bread", TestDates.Oct(3), shelfLifeDays: 1),
            TestShoppingNeeds.Create("Tomato", TestDates.Oct(4), shelfLifeDays: 2));

        // Act
        var result = await _sut.Generate(Friday, strategy, CancellationToken.None);

        // Assert
        result.Value.Trips
            .Select(trip => $"{trip.Date.ToString("MM-dd", CultureInfo.InvariantCulture)}: {string.Join(", ", trip.Items.Select(x => x.IngredientName))}")
            .Should().Equal(expectedTrips);
    }

    [Fact]
    public async Task Generate_MapsThePlanToTheResponse()
    {
        // Arrange
        GivenNeeds(TestShoppingNeeds.Create("Rice", TestDates.Oct(3), shelfLifeDays: null, quantity: 250m, unit: MeasureUnit.Gram));

        // Act
        var result = await _sut.Generate(Friday, PlanningStrategyKind.LatestInWindow, CancellationToken.None);

        // Assert
        var list = result.Value;
        list.StartDate.Should().Be(Friday);

        var trip = list.Trips.Should().ContainSingle().Which;
        trip.Date.Should().Be(Friday);

        var item = trip.Items.Should().ContainSingle().Which;
        item.IngredientName.Should().Be("Rice");
        item.Quantity.Should().Be(250m);
        item.Unit.Should().BeEquivalentTo(new MeasureUnitDto("g", "Gram"));
        item.ShelfLifeWarning.Should().BeFalse();
    }

    [Theory]
    [InlineData(PlanningStrategyKind.ReuseExistingTrip)]
    [InlineData(PlanningStrategyKind.LatestInWindow)]
    public async Task Generate_EchoesTheRequestedStrategyInTheResponse(PlanningStrategyKind strategy)
    {
        // Act
        var result = await _sut.Generate(Friday, strategy, CancellationToken.None);

        // Assert
        result.Value.Strategy.Should().Be(strategy);
    }

    [Fact]
    public async Task Generate_PassesTheClosedDaysToThePlanner()
    {
        // Arrange: milk for Tuesday 6 October (shelf life 1) would be bought on Monday 5 October, but that day is closed,
        // so it falls back to the latest earlier trip, Saturday 3 October, with a warning
        GivenClosedDays(TestDates.Oct(5));
        GivenNeeds(TestShoppingNeeds.Create("Milk", TestDates.Oct(6), shelfLifeDays: 1));

        // Act
        var result = await _sut.Generate(Friday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        var trip = result.Value.Trips.Should().ContainSingle().Which;
        trip.Date.Should().Be(TestDates.Oct(3));
        trip.Items.Should().ContainSingle().Which.ShelfLifeWarning.Should().BeTrue();
    }

    [Theory]
    [InlineData("pl", "Czosnek", "Ćwikła")]
    [InlineData("en", "Ćwikła", "Czosnek")]
    public async Task Generate_SortsItemsByNameUsingTheRequestCulture(string cultureName, string first, string second)
    {
        // Arrange: in Polish "Ć" is a letter of its own after "C", in English it sorts as "C" with an accent
        GivenNeeds(
            TestShoppingNeeds.Create("Czosnek", TestDates.Oct(3), shelfLifeDays: null),
            TestShoppingNeeds.Create("Ćwikła", TestDates.Oct(3), shelfLifeDays: null));
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

            // Act
            var result = await _sut.Generate(Friday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

            // Assert
            result.Value.Trips.Should().ContainSingle().Which.Items.Select(x => x.IngredientName).Should().Equal(first, second);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task Generate_Fails_WhenTheNeedsSourceFails()
    {
        // Arrange
        var error = Error.Validation("NeedsSourceFailed");
        _needsSource.Setup(x => x.GetShoppingNeeds(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ShoppingNeed>>(error));

        // Act
        var result = await _sut.Generate(Friday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo([error]);
    }

    [Fact]
    public async Task Generate_Fails_WhenTheClosedDaysProviderFails()
    {
        // Arrange
        var error = Error.Validation("ClosedDaysFailed");
        _closedDaysProvider.Setup(x => x.GetClosedDays(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<HashSet<DateOnly>>(error));

        // Act
        var result = await _sut.Generate(Friday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo([error]);
    }

    [Fact]
    public async Task Generate_Fails_WhenThereIsNoOpenDayForTheBigShop()
    {
        // Arrange: every day from F-6 (26 September) to F+6 (8 October) is closed
        var closed = Enumerable.Range(0, 13).Select(offset => new DateOnly(2026, 9, 26).AddDays(offset)).ToArray();
        GivenClosedDays(closed);

        // Act
        var result = await _sut.Generate(Friday, PlanningStrategyKind.ReuseExistingTrip, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Shopping.NoOpenBigShopDay(Friday));
    }

    // start, needs from, needs to, closed days from, closed days to
    public static TheoryData<DateOnly, DateOnly, DateOnly, DateOnly, DateOnly> RangeCases => new()
    {
        { TestDates.Oct(2), TestDates.Oct(3), TestDates.Oct(9), new DateOnly(2026, 9, 26), TestDates.Oct(9) },
        { TestDates.Oct(7), TestDates.Oct(8), TestDates.Oct(14), TestDates.Oct(1), TestDates.Oct(14) },
    };

    public static TheoryData<PlanningStrategyKind, string[]> StrategyCases => new()
    {
        // ReuseExistingTrip puts the tomatoes on the Friday trip that bread already makes necessary
        { PlanningStrategyKind.ReuseExistingTrip, ["10-02: Bread, Tomato"] },
        // LatestInWindow buys them as late as possible, on Saturday
        { PlanningStrategyKind.LatestInWindow, ["10-02: Bread", "10-03: Tomato"] },
    };

    private void GivenNeeds(params ShoppingNeed[] needs) =>
        _needsSource.Setup(x => x.GetShoppingNeeds(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(needs.ToList()));

    private void GivenClosedDays(params DateOnly[] closedDays) =>
        _closedDaysProvider.Setup(x => x.GetClosedDays(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(closedDays.ToHashSet()));
}
