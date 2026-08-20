using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Ingredients;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeIngredientDeleterTests
{
    private readonly RecipeIngredientDeleter _sut;

    private readonly List<Recipe> _recipes = [];

    public RecipeIngredientDeleterTests()
    {
        var localizer = new Mock<IStringLocalizer<Translations>>();
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        var measureUnitMapper = new MeasureUnitMapper(localizer.Object);
        _sut = new RecipeIngredientDeleter(ctx.Object, measureUnitMapper);
    }

    [Fact]
    public async Task DeleteIngredient_Throws_WhenRecipeWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        var usedIngredient = recipe.Ingredients[0]; 
        
        // Act
        var updateIngredient = () => _sut.DeleteIngredient(recipe.Id, usedIngredient.IngredientId,
            usedIngredient.Unit.ToString(), CancellationToken.None);
        
        // Assert
        await updateIngredient.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Recipe could not be found");
    }

    [Fact]
    public async Task DeleteIngredient_Throws_WhenIngredientWasNotFound()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0];
        
        // Act
        var updateIngredient = () => _sut.DeleteIngredient(recipe.Id, 999,
            usedIngredient.Unit.ToString(), CancellationToken.None);
        
        // Assert
        await updateIngredient.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Specified ingredient could not be found");
    }

    [Fact]
    public async Task DeleteIngredient_Throws_WhenMeasureUnitDoesNotMatch()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0]; 
        
        // Act
        var updateIngredient = () => _sut.DeleteIngredient(recipe.Id, usedIngredient.IngredientId,
            nameof(MeasureUnit.Slice2), CancellationToken.None);
        
        // Assert
        await updateIngredient.Invoking(x => x.Invoke())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Specified ingredient could not be found");
    }

    [Fact]
    public async Task DeleteIngredient_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        _recipes.Add(recipe);
        var usedIngredient = recipe.Ingredients[0];
        
        // Act
        await _sut.DeleteIngredient(recipe.Id, usedIngredient.IngredientId, usedIngredient.Unit.ToString(),
            CancellationToken.None);
        
        // Assert
        recipe.Ingredients.Should().NotContain(x => x.IngredientId == usedIngredient.IngredientId);
    }
}