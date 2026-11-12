using System.Globalization;
using MealPlanner.Shared.Shopping.Responses;

namespace MealPlanner.UI.Services;

/// <summary>
/// Turns a planned shopping trip into plain text: the trip date, then one item per line.
/// The same item text is shown on screen and put on the clipboard.
/// </summary>
public static class ShoppingTripFormatter
{
    // The UI does not reference the domain MeasureUnit enum, so the piece is recognized by its underlying value.
    private const string PieceUnitValue = "Piece";
    private const string WarningMarker = "⚠";

    public static string Format(PlannedTripResponse trip, CultureInfo culture)
    {
        // The date does not follow the culture: the copied text always starts with day.month.year.
        var header = trip.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

        return string.Join('\n', trip.Items.Select(item => FormatItem(item, culture)).Prepend(header));
    }

    public static string FormatItem(PlannedItemResponse item, CultureInfo culture, bool includeWarningMarker = true)
    {
        var text = $"{item.IngredientName} x{item.Quantity.ToString("0.##", culture)}";

        if (item.Unit.UnderlyingValue != PieceUnitValue)
        {
            text += $" {item.Unit.DisplayName}";
        }

        if (includeWarningMarker && item.ShelfLifeWarning)
        {
            text += $" {WarningMarker}";
        }

        return text;
    }
}
