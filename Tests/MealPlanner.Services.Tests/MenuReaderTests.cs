using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuReaderTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly MenuReader _sut;

    private static readonly List<Menu> _menus = [];
    public MenuReaderTests()
    {
        _menus.Clear();
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
        var result = await _sut.Get(1, CancellationToken.None);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = CreateAndSaveMenu(Today);
        
        // Act
        var result = await _sut.Get(menu.Id, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(menu.Id);
        result.Date.Should().Be(menu.Date);
    }

    [Fact]
    public async Task GetForDate_ReturnsNull_WhenMenuDoesNotExist()
    {
        // Act
        var result = await _sut.Get(Today, CancellationToken.None);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetForDate_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = CreateAndSaveMenu(Today);
        
        // Act
        var result = await _sut.Get(Today, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(menu.Id);
        result.Date.Should().Be(menu.Date);
    }
    
    [Theory]
    [MemberData(nameof(GetRangeTestData))]
    public async Task GetRange_Theory(Action arrange, DateOnly? from, DateOnly? to, int expectedCount)
    {
        // Arrange
        arrange();
        
        // Act
        var result = await _sut.GetRange(from, to, CancellationToken.None);
        
        // Assert
        result.ExistingMenus.Should().HaveCount(expectedCount);
    }
    
    public static TheoryData<Action, DateOnly?, DateOnly?, int> GetRangeTestData
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            
            return new TheoryData<Action, DateOnly?, DateOnly?, int>
            {
                { () => { CreateAndSaveMenu(today); CreateAndSaveMenu(tomorrow); }, null, null, 2 },
                { () => { CreateAndSaveMenu(today); }, tomorrow, tomorrow.AddDays(1), 0 },
                { () => { CreateAndSaveMenu(today); }, tomorrow, today, 0 },
                { () => { CreateAndSaveMenu(today); CreateAndSaveMenu(tomorrow); }, tomorrow, null, 1 },
                { () => { CreateAndSaveMenu(today); CreateAndSaveMenu(tomorrow); }, null, today, 1 },
                { () => { CreateAndSaveMenu(today); CreateAndSaveMenu(tomorrow); }, today, today, 1 }
            };
        }
    }
    
    [Fact]
    public async Task GetRange_ReturnsOnlyMenusWithinRange_WhenBothFromAndToProvided()
    {
        // Arrange
        CreateAndSaveMenu(Today.AddDays(-1));
        var menuInRange1 = CreateAndSaveMenu(Today);
        var menuInRange2 = CreateAndSaveMenu(Today.AddDays(1));
        CreateAndSaveMenu(Today.AddDays(2));
        
        // Act
        var result = await _sut.GetRange(Today, Today.AddDays(1), CancellationToken.None);
        
        // Assert
        result.ExistingMenus.Should().HaveCount(2);
        result.ExistingMenus.Should().Contain(m => m.Id == menuInRange1.Id);
        result.ExistingMenus.Should().Contain(m => m.Id == menuInRange2.Id);
    }
    
    [Fact]
    public async Task GetRange_HasMeals_Set_WhenMenuHasItems()
    {
        // Arrange
        var menuWithMeals = CreateAndSaveMenu(Today);
        menuWithMeals.AddMeal(Recipe.Create("Pizza"));
        
        var menuWithoutMeals = CreateAndSaveMenu(Today.AddDays(1));
        
        // Act
        var result = await _sut.GetRange(null, null, CancellationToken.None);
        
        // Assert
        result.ExistingMenus.Should().ContainSingle(m => m.Id == menuWithMeals.Id);
        result.ExistingMenus.Should().ContainSingle(m => m.Id == menuWithoutMeals.Id);
    }
    
    private static Menu CreateAndSaveMenu(DateOnly date)
    {
        var menu = TestMenu.Create(date);
        RandomId.Set(menu);
        _menus.Add(menu);
        return menu;
    }
}