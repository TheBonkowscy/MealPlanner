using MealPlanner.Domain;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuDeleterTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Test Recipe");
    private static readonly Menu PreExistingMenu = TestMenu.Create(Today, [AddMealAction.Create(PreExistingRecipe, 1, 1)]);
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