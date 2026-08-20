using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Exceptions;
using MealPlanner.Services.Recipes.Ingredients;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeIngredientUpdaterTests
{
    private readonly RecipeIngredientUpdater _sut;

    private readonly List<Recipe> _recipes = [];
    private readonly List<Ingredient> _ingredients = [];

    public RecipeIngredientUpdaterTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipeIngredientUpdater(ctx.Object, measureUnitMapper, new RecipeMapper(measureUnitMapper));
    }

    [Fact]
    public async Task UpdateIngredient_Throws_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var usedIngredient = recipe.Ingredients[0];
        var request = new UpdateRecipeIngredientRequest(usedIngredient.IngredientId,  usedIngredient.Quantity + 10, usedIngredient.Unit.ToString()); 
        
        // Act
        var updateIngredient = () => _sut.UpdateIngredient(recipe.Id, usedIngredient.IngredientId, request, CancellationToken.None);
        
        // Assert
        await updateIngredient.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<RecipeDoesNotExistException>();
    }

    [Fact]
    public async Task UpdateIngredient_AddsIngredient_WhenIngredientWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var newIngredient = TestIngredients.Create();
        
        var request = new UpdateRecipeIngredientRequest(newIngredient.Id,  10, newIngredient.ApplicableUnits.First().ToString()); 
        
        // Act
        var result = await _sut.UpdateIngredient(recipe.Id, request.Id, request, CancellationToken.None);
        
        // Assert
        var newIngredientResponse = result.Ingredients.First(x => x.Id == newIngredient.Id);
        newIngredientResponse.MeasureUnit.UnderlyingValue.Should().Be(request.Unit);
        newIngredientResponse.Quantity.Should().Be(request.Quantity);
    }

    [Fact]
    public async Task UpdateIngredient_UpdatesIngredient_WhenIngredientWasFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0];
        var request = new UpdateRecipeIngredientRequest(usedIngredient.IngredientId,  usedIngredient.Quantity + 10, usedIngredient.Unit.ToString()); 
        
        // Act
        var result = await _sut.UpdateIngredient(recipe.Id, request.Id, request, CancellationToken.None);
        
        // Assert
        var usedIngredientResponse = result.Ingredients.First(x => x.Id == usedIngredient.IngredientId);
        usedIngredientResponse.Quantity.Should().Be(request.Quantity);
    }

    [Fact]
    public async Task UpdateIngredient_Throws_WhenMeasureUnitDoesNotMatch()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0];
        var request = new UpdateRecipeIngredientRequest(999,  usedIngredient.Quantity + 10, nameof(MeasureUnit.Slice2)); 
        
        // Act
        var updateIngredient = () => _sut.UpdateIngredient(recipe.Id, usedIngredient.IngredientId, request, CancellationToken.None);
        
        // Assert
        await updateIngredient.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<IngredientDoesNotExistException>();
    }

    [Fact]
    public async Task UpdateIngredient_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0];
        var request = new UpdateRecipeIngredientRequest(usedIngredient.IngredientId,  usedIngredient.Quantity + 10, usedIngredient.Unit.ToString()); 
        
        // Act
        var result = await _sut.UpdateIngredient(recipe.Id, usedIngredient.IngredientId, request, CancellationToken.None);
        
        // Assert
        result.Name.Should().Be(recipe.Name);
        result.Servings.Should().Be(recipe.Servings);
        var responseIngredient = result.Ingredients.First(x => x.Id == usedIngredient.IngredientId);
        responseIngredient.Quantity.Should().Be(usedIngredient.Quantity);
    }
}