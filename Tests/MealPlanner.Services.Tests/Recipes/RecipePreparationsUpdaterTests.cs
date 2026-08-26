using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipePreparationsUpdaterTests
{
    private readonly RecipePreparationsUpdater _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipePreparationsUpdaterTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipePreparationsUpdater(ctx.Object, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task UpdatePreparation_Fails_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var prep = recipe.Preparations[0];
        var request = new UpdateRecipePreparationsRequest(prep.Id, prep.Description, prep.LeadDays, prep.Required); 
        
        // Act
        var result = await _sut.UpdatePreparations(recipe.Id, prep.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Recipe.DoesNotExist);
    }

    [Fact]
    public async Task UpdatePreparation_Fails_WhenStepWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var prep = recipe.Preparations[0];
        var request = new UpdateRecipePreparationsRequest(999, prep.Description, prep.LeadDays, prep.Required); 
        
        // Act
        var result = await _sut.UpdatePreparations(recipe.Id, 999, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipePreparation.NotFound);
    }

    [Fact]
    public async Task UpdatePreparation_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var prep = recipe.Preparations[0];
        var request = new UpdateRecipePreparationsRequest(prep.Id, prep.Description + "a", prep.LeadDays + 1, !prep.Required);
        
        // Act
        var result = await _sut.UpdatePreparations(recipe.Id, prep.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        var prepResponse = result.Value.Preparations.First(x => x.Id == prep.Id);
        prepResponse.Description.Should().Be(request.Description);
        prepResponse.Required.Should().Be(request.Required);
    }
}