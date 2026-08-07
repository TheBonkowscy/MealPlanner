using System.Collections;

namespace MealPlanner.Tests.Shared.Helpers;

public class EmptyStringTestDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [""];
        yield return [" "];
        yield return ["\r\n\t"];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}