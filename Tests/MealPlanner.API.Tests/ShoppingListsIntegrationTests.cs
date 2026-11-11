using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Services.Shopping;
using MealPlanner.Shared.Shopping;
using MealPlanner.Shared.Shopping.Responses;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class ShoppingListsIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // A Friday. No closed day of the calendar falls within a week of it, so the planning is not affected by the calendar.
    private static readonly DateOnly BigShopFriday = new(2026, 10, 23);

    [Fact]
    public async Task Get_ReturnsTripsForTheCycle()
    {
        // Arrange: a recipe for 2 servings planned for 4, so the 1 g of pasta it lists has to be doubled
        await AddPastaDishMenu(BigShopFriday.AddDays(1), plannedServings: 4);

        // Act
        var result = await Client.GetAsync(BuildRoute(BigShopFriday));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<ShoppingListResponse>();
        response.Should().NotBeNull();
        response.StartDate.Should().Be(BigShopFriday);
        var trip = response.Trips.Should().ContainSingle().Subject;
        trip.Date.Should().Be(BigShopFriday);
        var item = trip.Items.Should().ContainSingle().Subject;
        item.IngredientName.Should().Be("Pasta");
        item.Quantity.Should().Be(2);
        item.Unit.UnderlyingValue.Should().Be("Gram");
        item.ShelfLifeWarning.Should().BeFalse();
    }

    [Fact]
    public async Task Get_ReturnsEmptyTrips_WhenThereAreNoMenus()
    {
        // Act
        var result = await Client.GetAsync(BuildRoute(BigShopFriday));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<ShoppingListResponse>();
        response.Should().NotBeNull();
        response.Trips.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_UsesReuseExistingTrip_WhenStrategyIsNotProvided()
    {
        // Act
        var result = await Client.GetAsync(BuildRoute(BigShopFriday));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<ShoppingListResponse>();
        response.Should().NotBeNull();
        response.Strategy.Should().Be(PlanningStrategyKind.ReuseExistingTrip);
    }

    [Fact]
    public async Task Get_UsesTheRequestedStrategy_WhenStrategyIsProvided()
    {
        // Act
        var result = await Client.GetAsync($"{BuildRoute(BigShopFriday)}&strategy=LatestInWindow");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<ShoppingListResponse>();
        response.Should().NotBeNull();
        response.Strategy.Should().Be(PlanningStrategyKind.LatestInWindow);
    }

    [Theory]
    [MemberData(nameof(NonFridayStartDates))]
    public async Task Get_ReturnsOk_WhenStartDateIsNotFriday(DateOnly startDate)
    {
        // Arrange: the start day is the big shop day whatever its weekday
        await AddPastaDishMenu(startDate.AddDays(1), plannedServings: 2);

        // Act
        var result = await Client.GetAsync(BuildRoute(startDate));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<ShoppingListResponse>();
        response.Should().NotBeNull();
        response.StartDate.Should().Be(startDate);
        response.Trips.Should().ContainSingle().Which.Date.Should().Be(startDate);
    }

    public static TheoryData<DateOnly> NonFridayStartDates =>
    [
        new DateOnly(2026, 10, 19), // Monday
        new DateOnly(2026, 10, 24), // Saturday
        new DateOnly(2026, 10, 25) // Sunday
    ];

    [Fact]
    public async Task Get_ReturnsValidationProblem_WhenStartDateIsMissing()
    {
        // Act
        var result = await Client.GetAsync(Constants.ShoppingListsRoute);

        // Assert: the client is told which parameter is missing, rather than getting an error from inside the planner
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Keys.Should().Contain(key => key.EndsWith("StartDate", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("tomorrow")]
    [InlineData("2026-02-30")]
    public async Task Get_ReturnsBadRequest_WhenStartDateIsMalformed(string startDate)
    {
        // Act
        var result = await Client.GetAsync(BuildRoute(startDate));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("42")]
    public async Task Get_ReturnsBadRequest_WhenStrategyIsUnknown(string strategy)
    {
        // Act
        var result = await Client.GetAsync($"{BuildRoute(BigShopFriday)}&strategy={strategy}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("en", "No open day is available")]
    [InlineData("pl", "Brak dnia")]
    public async Task Get_ReturnsLocalizedBadRequest_WhenNoDayIsOpenForTheBigShop(string language, string expectedTitleStart)
    {
        // Arrange: every day the big shop could be moved to (six days either side of the start) is closed
        var closedDays = Enumerable.Range(-6, 13).Select(BigShopFriday.AddDays).ToArray();
        var client = CreateClient(services =>
        {
            services.RemoveAll<IClosedDaysProvider>();
            services.AddSingleton<IClosedDaysProvider>(new StaticClosedDaysProvider(closedDays));
        });
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);

        // Act
        var result = await client.GetAsync(BuildRoute(BigShopFriday));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Title.Should().StartWith(expectedTitleStart);
    }

    private async Task AddPastaDishMenu(DateOnly date, int plannedServings)
    {
        var pasta = TestIngredients.Create("Pasta", [MeasureUnit.Gram]);
        var recipe = TestRecipes.Create("Pasta dish", servings: 2, ingredients: [pasta]);
        await AddRecipeToDatabase(recipe);
        await AddMenuToDatabase(TestMenu.Create(date, [TestActions.AddMeal(recipe, 1, plannedServings)]));
    }

    private static string BuildRoute(DateOnly startDate) => BuildRoute(startDate.ToString("O"));

    private static string BuildRoute(string startDate) => $"{Constants.ShoppingListsRoute}?startDate={startDate}";
}
