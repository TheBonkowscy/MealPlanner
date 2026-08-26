using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
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

public class RecipePreparationsCreatorTests
{
    private readonly RecipePreparationsCreator _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipePreparationsCreatorTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipePreparationsCreator(ctx.Object, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task CreatePreparations_Fails_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var request = new CreateRecipePreparationsRequest("Thaw the meat in the morning, then marinade it overnight", 1, true); 
        
        // Act
        var result = await _sut.CreatePreparations(recipe.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Recipe.DoesNotExist);
    }
    
    [Fact]
    public async Task CreatePreparations_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var preparation = recipe.Preparations[0];
        var newLeadDays = preparation.LeadDays + 1;
        var newDescription = "Actually, forget it, let's do the prep 2 days earlier and see what happens";
        var request = new CreateRecipePreparationsRequest(newDescription, newLeadDays, false);
        
        // Act
        var result = await _sut.CreatePreparations(recipe.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        var prepResponse = result.Value.Preparations.First(x => x.LeadDays == newLeadDays);
        prepResponse.LeadDays.Should().Be(newLeadDays);
        prepResponse.Required.Should().BeFalse();
        prepResponse.Description.Should().Be(newDescription);
    }
}