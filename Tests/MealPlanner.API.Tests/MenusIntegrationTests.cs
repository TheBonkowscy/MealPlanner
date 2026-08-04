using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MenusIntegrationTests : IntegrationTestBase
{
    public MenusIntegrationTests(MealPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);
    private static readonly DateOnly SpecificDate = new(2026, 03, 26);
    private static readonly List<Meal> Meals = [Meal.Create("Breakfast")];
    
    [Fact]
    public async Task Post_ReturnsId()
    {
        // Arrange
        var request = new CreateMenuRequest(Tomorrow);
        
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
        var menu = Menu.Create(Tomorrow, Meals);
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
        var menu = Menu.Create(SpecificDate, Meals);
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
        var menu = Menu.Create(Today, Meals);
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
        await AddMenuToDatabase(Menu.Create(Today, Meals));
        await AddMenuToDatabase(Menu.Create(Tomorrow, Meals));
        
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
        await AddMenuToDatabase(Menu.Create(Today.AddDays(-1), Meals));
        var menu1 = Menu.Create(Today, Meals);
        var menu2 = Menu.Create(Tomorrow, Meals);
        await AddMenuToDatabase(menu1);
        await AddMenuToDatabase(menu2);
        await AddMenuToDatabase(Menu.Create(Tomorrow.AddDays(1), Meals));

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

    private static string BuildGetRoute(int id) => $"{Constants.MenusRoute}/{id.ToString()}";

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";
    
    private static string BuildGetForTodayRoute() => $"{Constants.MenusRoute}/today";

    private async Task AddMenuToDatabase(Menu menu)
    {
        await DatabaseContext.Menus.AddAsync(menu);
        await DatabaseContext.SaveChangesAsync();
    }
}