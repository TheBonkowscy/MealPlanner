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

public class RecipeStepCreatorTests
{
    private readonly RecipeStepCreator _sut;

    private readonly List<Recipe> _recipes = [];
    private readonly List<RecipeStep> _steps = [];

    public RecipeStepCreatorTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        ctx.Setup(x => x.RecipeSteps).ReturnsDbSet(_steps);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipeStepCreator(ctx.Object, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task CreateStep_Throws_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var request = new CreateRecipeStepRequest(2, "Instructions for step #2"); 
        
        // Act
        var updateStep = () => _sut.CreateStep(recipe.Id, request, CancellationToken.None);
        
        // Assert
        await updateStep.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Recipe could not be found");
    }

    [Fact]
    public async Task CreateStep_Throws_WhenOrderAlreadyExists()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var request = new CreateRecipeStepRequest(1, "Instructions for step #1"); 
        
        // Act
        var createStep = () => _sut.CreateStep(recipe.Id, request, CancellationToken.None);
        
        // Assert
        await createStep.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("There is already a step with this order!");
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
        var stepResponse = result.Steps.First(x => x.Order == newOrder);
        stepResponse.Instructions.Should().Be(newInstructions);
    }
}