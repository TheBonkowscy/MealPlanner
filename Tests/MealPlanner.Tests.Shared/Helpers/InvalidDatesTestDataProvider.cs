using System.Collections;
using MealPlanner.Domain;
using MealPlanner.Domain.Menus;

namespace MealPlanner.Tests.Shared.Helpers;

public class InvalidDatesTestDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [DateOnly.MinValue, DomainErrors.Menu.DateIsUnset];
        yield return [DateOnly.MaxValue, DomainErrors.Menu.DateIsUnset];
        yield return [Menu.MinDateInThePast.AddDays(-1), DomainErrors.Menu.DateTooFarInThePast];
        yield return [DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1), DomainErrors.Menu.DateTooFarInTheFuture];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}