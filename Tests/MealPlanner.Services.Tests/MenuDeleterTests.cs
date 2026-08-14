using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuDeleterTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Test Meal");
    private static readonly Menu PreExistingMenu = Menu.Create(Today, [PreExistingRecipe]);
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly MenuDeleter _sut;
    
    public MenuDeleterTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet([PreExistingMenu]);
        _ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        
        _sut = new MenuDeleter(_ctx.Object);
    }
    
    [Fact]
    public async Task Deletes_WhenMenuDoesNotExist()
    {
        // Act
        await _sut.Delete(Today.AddDays(1), CancellationToken.None);
        
        // Assert
        _ctx.Verify(x => x.Menus.Remove(It.IsAny<Menu>()), Times.Never);
    }
    
    [Fact]
    public async Task Deletes_WhenMenuExists()
    {
        // Act
        await _sut.Delete(DateOnly.FromDateTime(DateTime.Today), CancellationToken.None);
        
        // Assert
        _ctx.Verify(x => x.Menus.Remove(It.IsAny<Menu>()), Times.Once);
    }
}