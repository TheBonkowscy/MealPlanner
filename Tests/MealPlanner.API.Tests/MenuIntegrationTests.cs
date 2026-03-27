using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Menus;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Xunit;

namespace MealPlanner.API.Tests;

public class MenuIntegrationTests : IntegrationTestBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);
    private static readonly DateOnly SpecificDate = new(2026, 03, 26);
    
    [Fact]
    public async Task Post_ReturnsId()
    {
        // Arrange
        var request = new CreateMenuRequest(Tomorrow);
        
        // Act
        var result = await Client.PostAsJsonAsync(Constants.MenuRoute, request);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<CreateMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().NotBe(0);
    }

    [Fact]
    public async Task Get_ById_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = Menu.Create(Tomorrow);
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
        var menu = Menu.Create(SpecificDate);
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
        var menu = Menu.Create(Today);
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

    private static string BuildGetRoute(int id) => $"{Constants.MenuRoute}/{id.ToString()}";

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenuRoute}/{date.ToString("O")}";
    
    private static string BuildGetForTodayRoute() => $"{Constants.MenuRoute}/today";

    private async Task AddMenuToDatabase(Menu menu)
    {
        await DatabaseContext.Menus.AddAsync(menu);
        await DatabaseContext.SaveChangesAsync();
    }
}