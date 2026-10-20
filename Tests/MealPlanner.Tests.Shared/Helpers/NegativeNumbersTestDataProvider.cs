using System.Collections;

namespace MealPlanner.Tests.Shared.Helpers;

public class NegativeNumbersTestDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [-1];
        yield return [int.MinValue];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}