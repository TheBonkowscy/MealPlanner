using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Meals;
using MealPlanner.Shared.Menus;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MealsIntegrationTests : IntegrationTestBase
{
    public MealsIntegrationTests(MealPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_ReturnsAllMeals_WhenNoQueryProvided()
    {
        // Arrange
        await AddMealToDatabase(Meal.Create("Pizza"));
        await AddMealToDatabase(Meal.Create("Burger"));

        // Act
        var result = await Client.GetAsync(Constants.MealsRoute);

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMealsResponse>();
        response.Should().NotBeNull();
        response.Meals.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_ReturnsFilteredMeals_WhenQueryProvided()
    {
        // Arrange
        await AddMealToDatabase(Meal.Create("Pizza"));
        await AddMealToDatabase(Meal.Create("Burger"));

        // Act
        var result = await Client.GetAsync($"{Constants.MealsRoute}?q=pi");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMealsResponse>();
        response.Should().NotBeNull();
        response.Meals.Should().HaveCount(1);
        response.Meals.Should().Contain(m => m.Name == "Pizza");
    }

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoMatchesFound()
    {
        // Arrange
        await AddMealToDatabase(Meal.Create("Pizza"));

        // Act
        var result = await Client.GetAsync($"{Constants.MealsRoute}?q=salad");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMealsResponse>();
        response.Should().NotBeNull();
        response!.Meals.Should().BeEmpty();
    }

    private async Task AddMealToDatabase(Meal meal)
    {
        await DatabaseContext.Meals.AddAsync(meal);
        await DatabaseContext.SaveChangesAsync();
    }
}
