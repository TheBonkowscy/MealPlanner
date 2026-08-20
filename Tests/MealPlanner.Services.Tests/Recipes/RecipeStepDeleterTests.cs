using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes.Steps;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeStepDeleterTests
{
    private readonly RecipeStepDeleter _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipeStepDeleterTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        _sut = new RecipeStepDeleter(ctx.Object);
    }

    [Fact]
    public async Task DeleteStep_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var step = recipe.Steps[0];
        
        // Act
        await _sut.DeleteStep(recipe.Id, step.Id, CancellationToken.None);
        
        // Assert
        var recipeResult = _recipes.FirstOrDefault(x => x.Id == recipe.Id);
        var stepResult = recipeResult!.Steps.FirstOrDefault(x => x.Id == step.Id);
        stepResult.Should().BeNull();
    }
}