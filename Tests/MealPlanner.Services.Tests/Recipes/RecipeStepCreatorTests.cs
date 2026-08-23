using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Steps;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeStepCreatorTests
{
    private readonly RecipeStepCreator _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipeStepCreatorTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipeStepCreator(ctx.Object, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task CreateStep_Fails_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var request = new CreateRecipeStepRequest(2, "Instructions for step #2"); 
        
        // Act
        var result = await _sut.CreateStep(recipe.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Recipe.DoesNotExist);
    }
    
    [Fact]
    public async Task CreateStep_InsertsStepAndReordersExisting_WhenOrderAlreadyExists()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var request = new CreateRecipeStepRequest(1, "New First Step"); 
    
        // Act
        var result = await _sut.CreateStep(recipe.Id, request, CancellationToken.None);
    
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        recipe.Steps.Should().HaveCount(2);
        recipe.Steps.First(s => s.Order == 1).Instructions.Should().Be("New First Step");
        recipe.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task CreateStep_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var step = recipe.Steps[0];
        var newOrder = step.Order + 1;
        var newInstructions = "Reviewed instructions to prepare this dish";
        var request = new CreateRecipeStepRequest(newOrder, newInstructions);
        
        // Act
        var result = await _sut.CreateStep(recipe.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        var stepResponse = result.Value.Steps.First(x => x.Order == newOrder);
        stepResponse.Instructions.Should().Be(newInstructions);
    }
}