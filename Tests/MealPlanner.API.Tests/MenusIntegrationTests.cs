using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MenusIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);
    private static readonly DateOnly SpecificDate = new(2026, 03, 26);
    private static readonly Meal PreExistingMeal = Meal.Create("Breakfast");
    private static readonly Dictionary<int, string> Meals = new()
    {
        { 0, PreExistingMeal.Name }
    };
    
    [Fact]
    public async Task Post_ReturnsId()
    {
        // Arrange
        await AddMealToDatabase(PreExistingMeal);
        var request = new CreateMenuRequest(Tomorrow, Meals);
        
        // Act
        var result = await Client.PostAsJsonAsync(Constants.MenusRoute, request);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<CreateMenuResponse>();
        response.Should().NotBeNull();
        response.Date.Should().Be(Tomorrow);
    }

    [Fact]
    public async Task Get_ById_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = Menu.Create(Tomorrow, [.. Meals.Values.Select(Meal.Create)]);
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetRoute(menu.Id));
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(menu.Date);
    }
    
    [Fact]
    public async Task Get_ById_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetRoute(1));
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Get_ForSpecificDate_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetRoute(SpecificDate));
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ForSpecificDate_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = Menu.Create(SpecificDate, [..Meals.Values.Select(Meal.Create)]);
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetRoute(menu.Date));
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(menu.Date);
    }
    
    [Fact]
    public async Task Get_ForToday_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetForTodayRoute());
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ForToday_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = Menu.Create(Today, [..Meals.Values.Select(Meal.Create)]);
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetForTodayRoute());
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(Today);
    }

    [Theory]
    [MemberData(nameof(DateRangeTestData))]
    public async Task Get_ForDateRange(string query, int expectedCount)
    {
        // Arrange
        await AddMenuToDatabase(Menu.Create(Today, [..Meals.Values.Select(Meal.Create)]));
        await AddMenuToDatabase(Menu.Create(Tomorrow, [..Meals.Values.Select(Meal.Create)]));
        
        // Act
        var result = await Client.GetAsync($"{Constants.MenusRoute}{query}");
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        response.Should().NotBeNull();
        response.ExistingMenus.Should().HaveCount(expectedCount);
    }
    
    public static TheoryData<string, int> DateRangeTestData
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            return new TheoryData<string, int>
            {
                { $"?from={today:O}&to={tomorrow:O}", 2 },
                { $"?from={tomorrow.AddDays(1):O}&to={tomorrow.AddDays(2):O}", 0 },
                { $"?from={tomorrow:O}&to={today:O}", 0 },
                { $"?from={tomorrow:O}", 1 },
                { $"?to={today:O}", 1 },
                { $"?from={today:O}&to={today:O}", 1 }
            };
        }
    }

    [Fact]
    public async Task Get_ForDateRange_ReturnsOnlyMenusWithinRange_WhenBothFromAndToProvided()
    {
        // Arrange
        await AddMenuToDatabase(Menu.Create(Today.AddDays(-1), [..Meals.Values.Select(Meal.Create)]));
        var menu1 = Menu.Create(Today, [..Meals.Values.Select(Meal.Create)]);
        var menu2 = Menu.Create(Tomorrow, [..Meals.Values.Select(Meal.Create)]);
        await AddMenuToDatabase(menu1);
        await AddMenuToDatabase(menu2);
        await AddMenuToDatabase(Menu.Create(Tomorrow.AddDays(1), [..Meals.Values.Select(Meal.Create)]));

        // Act
        var result = await Client.GetAsync($"{Constants.MenusRoute}?from={Today:O}&to={Tomorrow:O}");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        response.Should().NotBeNull();
        response.ExistingMenus.Should().HaveCount(2);
        response.ExistingMenus.Should().Contain(m => m.Id == menu1.Id);
        response.ExistingMenus.Should().Contain(m => m.Id == menu2.Id);
    }
    [Fact]
    public async Task Put_Edit_UpdatesExistingMenu_WhenMenuExists()
    {
        // Arrange
        var initialMeal = Meal.Create("Obiad");
        var updatedMeal = Meal.Create("Kolacja");
        await AddMealToDatabase(initialMeal);
        await AddMealToDatabase(updatedMeal);

        var existingMenu = Menu.Create(SpecificDate, [initialMeal]);
        await AddMenuToDatabase(existingMenu);

        var updatedMealsDict = new Dictionary<int, string>
        {
            { 0, updatedMeal.Name }
        };
        var request = new UpdateMenuRequest(SpecificDate, updatedMealsDict);

        // Act
        var result = await Client.PutAsJsonAsync(BuildEditRoute(SpecificDate), request);

        // Assert
        result.EnsureSuccessStatusCode();

        var getRequest = await Client.GetAsync(BuildGetRoute(SpecificDate));
        var getResult = await getRequest.Content.ReadFromJsonAsync<GetMenuResponse>();

        getResult.Should().NotBeNull();
        getResult.Meals.Should().HaveCount(1);
        getResult.Meals.First().Value.Should().Be(updatedMeal.Name);
    }

    [Fact]
    public async Task Put_Edit_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Arrange
        var request = new UpdateMenuRequest(SpecificDate, Meals);

        // Act
        var result = await Client.PutAsJsonAsync(BuildEditRoute(SpecificDate), request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_Edit_ReturnsBadRequest_WhenRouteDateDoesNotMatchRequestBodyDate()
    {
        // Arrange
        var request = new UpdateMenuRequest(Tomorrow, Meals);

        // Act
        var result = await Client.PutAsJsonAsync(BuildEditRoute(SpecificDate), request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static string BuildGetRoute(int id) => $"{Constants.MenusRoute}/{id.ToString()}";

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";

    private static string BuildEditRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";
    
    private static string BuildGetForTodayRoute() => $"{Constants.MenusRoute}/today";

    private async Task AddMenuToDatabase(Menu menu)
    {
        await DatabaseContext.Menus.AddAsync(menu);
        await DatabaseContext.SaveChangesAsync();
    }
    
    private async Task AddMealToDatabase(Meal meal)
    {
        await DatabaseContext.Meals.AddAsync(meal);
        await DatabaseContext.SaveChangesAsync();
    }
}