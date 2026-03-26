using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Services.DailyMenus.Create;
using Xunit;
using CreateResponse = MealPlanner.Services.DailyMenus.Create.Response;
using Create = MealPlanner.API.DailyMenus.Create;
using ReadResponse = MealPlanner.Services.DailyMenus.Read.Response;
using Read = MealPlanner.API.DailyMenus.Read;

namespace MealPlanner.API.Tests;

public class DailyMenuIntegrationTests : IntegrationTestBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    
    [Fact]
    public async Task Post_DailyMenu_ReturnsId()
    {
        // Arrange
        var request = new Request(Today);
        
        // Act
        var result = await Client.PostAsJsonAsync(Create.Endpoint.Address, request);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<CreateResponse>();
        response.Should().NotBeNull();
        response.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Get_DailyMenu_ReturnsDailyMenuIfExists()
    {
        // Arrange
        var dailyMenu = DailyMenu.Create(Today);
        ctx.Database[dailyMenu.Id] = dailyMenu;
        
        // Act
        var result = await Client.GetAsync(Read.ByIdEndpoint.Address.Replace("{id:guid}", dailyMenu.Id.ToString()));
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<ReadResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(dailyMenu.Id);
        response.Date.Should().Be(Today);
    }
    
    [Fact]
    public async Task Get_ReturnsNotFound_WhenDailyMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(Read.ByIdEndpoint.Address.Replace("{id:guid}", Guid.NewGuid().ToString()));
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}