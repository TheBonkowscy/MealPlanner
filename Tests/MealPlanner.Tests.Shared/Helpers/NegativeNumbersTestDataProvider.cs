using System.Collections;

namespace MealPlanner.Tests.Shared.Helpers;

public class NegativeNumbersTestDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [0];
        yield return [-1];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}