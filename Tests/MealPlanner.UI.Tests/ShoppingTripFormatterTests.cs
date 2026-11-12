using System.Globalization;
using AwesomeAssertions;
using MealPlanner.Shared.Shared;
using MealPlanner.Shared.Shopping.Responses;
using MealPlanner.UI.Services;

namespace MealPlanner.UI.Tests;

public class ShoppingTripFormatterTests
{
    private static readonly CultureInfo Polish = new("pl-PL");
    private static readonly CultureInfo English = new("en-US");
    private static readonly DateOnly TripDate = new(2026, 10, 22);

    private static readonly MeasureUnitDto Piece = new("pc(s)", "Piece");
    private static readonly MeasureUnitDto Liter = new("l", "Liter");
    private static readonly MeasureUnitDto Gram = new("g", "Gram");

    [Theory]
    [InlineData("pl-PL")]
    [InlineData("en-US")]
    [InlineData("th-TH")] // Buddhist calendar: the year would be 2569 if the culture leaked into the date
    public void Format_StartsWithTheDateAsDayMonthYear(string cultureName)
    {
        var trip = Trip(Item("Bułka", 2, Piece));

        var firstLine = ShoppingTripFormatter.Format(trip, new CultureInfo(cultureName)).Split('\n')[0];

        firstLine.Should().Be("22.10.2026");
    }

    [Theory]
    [InlineData("pc(s)")]
    [InlineData("szt.")] // the label is localized; only the underlying value identifies a piece
    public void Format_RendersPiecesWithoutUnit(string pieceLabel)
    {
        var trip = Trip(Item("Bułka", 2, new MeasureUnitDto(pieceLabel, "Piece")));

        var result = ShoppingTripFormatter.Format(trip, English);

        result.Should().Be("22.10.2026\nBułka x2");
    }

    [Fact]
    public void Format_AppendsUnitForOtherUnits()
    {
        var trip = Trip(Item("Mleko", 1, Liter), Item("Kiełbasa", 200, Gram));

        var result = ShoppingTripFormatter.Format(trip, English);

        result.Should().Be("22.10.2026\nMleko x1 l\nKiełbasa x200 g");
    }

    [Fact]
    public void Format_AppendsWarningMarkerAfterTheUnit()
    {
        var trip = Trip(Item("Pomidor", 3, Piece, warning: true), Item("Mleko", 1, Liter, warning: true));

        var result = ShoppingTripFormatter.Format(trip, English);

        result.Should().Be("22.10.2026\nPomidor x3 ⚠\nMleko x1 l ⚠");
    }

    [Theory]
    [InlineData("pl-PL", "Mleko x1,5 l")]
    [InlineData("en-US", "Mleko x1.5 l")]
    public void Format_UsesTheCultureForQuantities(string cultureName, string expectedItem)
    {
        var trip = Trip(Item("Mleko", 1.5m, Liter));

        var lastLine = ShoppingTripFormatter.Format(trip, new CultureInfo(cultureName)).Split('\n')[1];

        lastLine.Should().Be(expectedItem);
    }

    [Theory]
    [InlineData("1.50", "Mleko x1,5 l")]
    [InlineData("2.00", "Mleko x2 l")]
    [InlineData("0.333", "Mleko x0,33 l")]
    public void Format_DropsTrailingZerosAndRoundsToTwoDecimals(string quantity, string expectedItem)
    {
        // decimal.Parse keeps the scale, so "1.50" really is 1.50m and not 1.5m.
        var trip = Trip(Item("Mleko", decimal.Parse(quantity, CultureInfo.InvariantCulture), Liter));

        var item = ShoppingTripFormatter.Format(trip, Polish).Split('\n')[1];

        item.Should().Be(expectedItem);
    }

    [Fact]
    public void Format_JoinsLinesWithNewLineAndKeepsItemOrder()
    {
        // Deliberately not alphabetical: the order of the items is the order of the trip.
        var trip = Trip(Item("Mleko", 1, Liter), Item("Bułka", 2, Piece), Item("Kiełbasa", 200, Gram));

        var result = ShoppingTripFormatter.Format(trip, English);

        result.Should().Be("22.10.2026\nMleko x1 l\nBułka x2\nKiełbasa x200 g");
    }

    [Fact]
    public void FormatItem_CanOmitTheWarningMarker()
    {
        var item = Item("Pomidor", 3, Piece, warning: true);

        var result = ShoppingTripFormatter.FormatItem(item, English, includeWarningMarker: false);

        result.Should().Be("Pomidor x3");
    }

    [Fact]
    public void FormatItem_AppendsWarningMarkerByDefault()
    {
        var item = Item("Pomidor", 3, Piece, warning: true);

        var result = ShoppingTripFormatter.FormatItem(item, English);

        result.Should().Be("Pomidor x3 ⚠");
    }

    private static PlannedItemResponse Item(string name, decimal quantity, MeasureUnitDto unit, bool warning = false) =>
        new(name, unit, quantity, warning);

    private static PlannedTripResponse Trip(params PlannedItemResponse[] items) => new(TripDate, items);
}
