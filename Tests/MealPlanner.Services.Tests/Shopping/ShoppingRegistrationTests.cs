using AwesomeAssertions;
using MealPlanner.Services.Shopping;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Services.Tests.Shopping;

public class ShoppingRegistrationTests
{
    [Fact]
    public async Task ClosedDaysProvider_ResolvesFromTheContainer_AndServesTheShippedCalendar()
    {
        // Arrange
        var services = new ServiceCollection();
        await services.RegisterMenuServices();
        await using var container = services.BuildServiceProvider();

        // Act
        var sut = container.GetRequiredService<IClosedDaysProvider>();
        var result = await sut.GetClosedDays(DateOnly.MinValue, DateOnly.MaxValue, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(ClosedDaysCalendar.Dates);
    }
}
