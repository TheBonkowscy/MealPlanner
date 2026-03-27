using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Read;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuReaderTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly MenuReader _sut;

    private static List<Menu> _menus = [];
    public MenuReaderTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
        {
            RandomId.Set(menu);
            _menus.Add(menu);
        });
        
        _ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        
        _sut = new MenuReader(_ctx.Object);
    }
    
    [Fact]
    public async Task GetById_ReturnsNull_WhenMenuDoesNotExist()
    {
        // Act
        var result = await _sut.Get(1);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = CreateAndSaveMenu(Today);
        
        // Act
        var result = await _sut.Get(menu.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(menu.Id);
        result.Date.Should().Be(menu.Date);
    }

    [Fact]
    public async Task GetForDate_ReturnsNull_WhenMenuDoesNotExist()
    {
        // Act
        var result = await _sut.Get(Today);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetForDate_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = CreateAndSaveMenu(Today);
        
        // Act
        var result = await _sut.Get(Today);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(menu.Id);
        result.Date.Should().Be(menu.Date);
    }
    
    private static Menu CreateAndSaveMenu(DateOnly date)
    {
        var menu = Menu.Create(date);
        RandomId.Set(menu);
        _menus.Add(menu);
        return menu;
    }
}