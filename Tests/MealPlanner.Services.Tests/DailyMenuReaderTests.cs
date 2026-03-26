using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Services.DailyMenus.Read;

namespace MealPlanner.Services.Tests;

public class DailyMenuReaderTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    
    private readonly InMemoryDatabase _ctx;
    private readonly DailyMenuReader _sut;

    public DailyMenuReaderTests()
    {
        _ctx = new InMemoryDatabase();
        _sut = new DailyMenuReader(_ctx);
    }
    
    [Fact]
    public async Task GetById_ReturnsNull_WhenDailyMenuDoesNotExist()
    {
        // Arrange 
        var id = Guid.NewGuid();
        
        // Act
        var result = await _sut.Get(id);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_ReturnsDailyMenuIfExists()
    {
        // Arrange
        var dailyMenu = DailyMenu.Create(Today);
        _ctx.Database[dailyMenu.Id] = dailyMenu;
        
        // Act
        var result = await _sut.Get(dailyMenu.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(dailyMenu.Id);
        result.Date.Should().Be(dailyMenu.Date);
    }

    [Fact]
    public async Task GetForDate_ReturnsNull_WhenDailyMenuDoesNotExist()
    {
        // Arrange 
        var id = Guid.NewGuid();
        
        // Act
        var result = await _sut.Get(Today);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetForDate_ReturnsDailyMenuIfExists()
    {
        // Arrange
        var dailyMenu = DailyMenu.Create(Today);
        _ctx.Database[dailyMenu.Id] = dailyMenu;
        
        // Act
        var result = await _sut.Get(Today);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(dailyMenu.Id);
        result.Date.Should().Be(dailyMenu.Date);
    }
    
}