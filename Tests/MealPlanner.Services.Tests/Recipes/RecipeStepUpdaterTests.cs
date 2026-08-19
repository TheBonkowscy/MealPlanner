using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Steps;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeStepUpdaterTests
{
    private readonly RecipeStepUpdater _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipeStepUpdaterTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipeStepUpdater(ctx.Object, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task UpdateStep_Throws_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var step = recipe.Steps[0];
        var request = new UpdateRecipeStepRequest(step.Id,  step.Order, step.Instructions); 
        
        // Act
        var updateStep = () => _sut.UpdateStep(recipe.Id, step.Id, request, CancellationToken.None);
        
        // Assert
        await updateStep.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Recipe could not be found");
    }

    [Fact]
    public async Task UpdateStep_Throws_WhenStepWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var step = recipe.Steps[0];
        var request = new UpdateRecipeStepRequest(999,  step.Order, step.Instructions); 
        
        // Act
        var updateStep = () => _sut.UpdateStep(recipe.Id, request.Id, request, CancellationToken.None);
        
        // Assert
        await updateStep.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Recipe step could not be found");
    }

    [Fact]
    public async Task UpdateStep_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var step = recipe.Steps[0];
        var request = new UpdateRecipeStepRequest(step.Id,  step.Order, step.Instructions + "a");
        
        // Act
        var result = await _sut.UpdateStep(recipe.Id, request.Id, request, CancellationToken.None);
        
        // Assert
        var stepResponse = result.Steps.First(x => x.Id == step.Id);
        stepResponse.Instructions.Should().Be(request.Instructions);
    }
}