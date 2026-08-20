using MealPlanner.Domain;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeDeleterTests
{
    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Test Recipe");
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly RecipeDeleter _sut;
    
    public RecipeDeleterTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Recipes).ReturnsDbSet([PreExistingRecipe]);
        _ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        
        _sut = new RecipeDeleter(_ctx.Object);
    }
    
    [Fact]
    public async Task Deletes_WhenMenuDoesNotExist()
    {
        // Act
        await _sut.Delete(Random.Shared.Next(99, 1000), CancellationToken.None);
        
        // Assert
        _ctx.Verify(x => x.Recipes.Remove(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task Deletes_WhenMenuExists()
    {
        // Act
        await _sut.Delete(PreExistingRecipe.Id, CancellationToken.None);
        
        // Assert
        _ctx.Verify(x => x.Recipes.Remove(It.IsAny<Recipe>()), Times.Once);
    }
}