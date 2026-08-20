using System.Collections;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Exceptions;

namespace MealPlanner.Tests.Shared.Helpers;

public class InvalidDatesTestDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [DateOnly.MinValue, DateOutOfRangeException.Cause.Unset];
        yield return [DateOnly.MaxValue, DateOutOfRangeException.Cause.Unset];
        yield return [Menu.MinDateInThePast.AddDays(-1), DateOutOfRangeException.Cause.TooFarInThePast];
        yield return [DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1), DateOutOfRangeException.Cause.TooFarInTheFuture];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}