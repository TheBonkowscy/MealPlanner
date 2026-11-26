using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using MealPlanner.Client.Menus;
using MealPlanner.Client.Tests.Helpers;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Tests;

public class MenuClientTests
{
    public static TheoryData<HttpStatusCode> FailureStatuses =>
        new() { HttpStatusCode.NotFound, HttpStatusCode.InternalServerError };

    [Fact]
    public async Task GetWeek_RequestsWeekRouteWithIsoDate()
    {
        // Arrange
        var stub = new StubHttpMessageHandler(HttpStatusCode.OK, JsonContent.Create(GetWeekResponse.Empty));
        var client = CreateClient(stub);

        // Act
        await client.GetWeek(new DateOnly(2025, 1, 3), CancellationToken.None);

        // Assert
        stub.Request!.Method.Should().Be(HttpMethod.Get);
        stub.Request.RequestUri!.PathAndQuery.Should().Be($"{Constants.MenuWeeksRoute}/2025-01-03");
    }

    [Fact]
    public async Task GetWeek_ReturnsDeserialisedResponse_WhenSuccessful()
    {
        // Arrange
        var expected = new GetWeekResponse(
            [new GetMenuResponse(7, new DateOnly(2025, 1, 3), [new OrderedRecipeListItemResponse(1, 1, 2, "Soup")])],
            new DateOnly(2025, 1, 3));
        var stub = new StubHttpMessageHandler(HttpStatusCode.OK, JsonContent.Create(expected));
        var client = CreateClient(stub);

        // Act
        var result = await client.GetWeek(new DateOnly(2025, 1, 3), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(FailureStatuses))]
    public async Task GetWeek_ReturnsEmpty_WhenNotSuccessful(HttpStatusCode status)
    {
        // Arrange
        var stub = new StubHttpMessageHandler(status, ErrorBody());
        var client = CreateClient(stub);

        // Act
        var result = await client.GetWeek(new DateOnly(2025, 1, 3), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(GetWeekResponse.Empty);
    }

    [Fact]
    public async Task GetWeek_ForwardsCancellationToken()
    {
        // Arrange
        var stub = new StubHttpMessageHandler(HttpStatusCode.InternalServerError, ErrorBody());
        var client = CreateClient(stub);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act
        var act = () => client.GetWeek(new DateOnly(2025, 1, 3), cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static MenuClient CreateClient(StubHttpMessageHandler stub) =>
        new(new HttpClient(stub) { BaseAddress = new Uri("http://localhost") });

    private static HttpContent ErrorBody() => JsonContent.Create(new { title = "error" });
}
