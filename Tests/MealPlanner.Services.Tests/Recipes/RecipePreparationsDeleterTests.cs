using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipePreparationsDeleterTests
{
    private readonly RecipePreparationsDeleter _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipePreparationsDeleterTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        _sut = new RecipePreparationsDeleter(ctx.Object);
    }

    [Fact]
    public async Task DeletePreparations_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var prep = recipe.Preparations[0];
        
        // Act
        await _sut.DeletePreparations(recipe.Id, prep.Id, CancellationToken.None);
        
        // Assert
        var recipeResult = _recipes.FirstOrDefault(x => x.Id == recipe.Id);
        var prepResult = recipeResult!.Preparations.FirstOrDefault(x => x.Id == prep.Id);
        prepResult.Should().BeNull();
    }
}