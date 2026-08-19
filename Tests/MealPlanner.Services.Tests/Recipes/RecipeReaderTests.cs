using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeReaderTests
{
    private readonly Mock<IStringLocalizer<Translations>> _localiser;
    
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly RecipeReader _sut;
    private readonly List<Recipe> _recipes = [];

    public RecipeReaderTests()
    {
        _localiser = new Mock<IStringLocalizer<Translations>>();
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        _sut = new RecipeReader(_ctx.Object, new RecipeMapper(new MeasureUnitMapper(_localiser.Object)));
    }

    [Fact]
    public async Task GetByQuery_NoQuery_ReturnsAllRecipes()
    {
        // Arrange
        var recipe1 = TestRecipes.Create("Pasta");
        RandomId.Set(recipe1);
        _recipes.Add(recipe1);
        
        var recipe2 = TestRecipes.Create("Pizza");
        RandomId.Set(recipe2);
        _recipes.Add(recipe2);

        // Act
        var result = await _sut.GetByQuery(null, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(2);
        result.Recipes.Should().Contain(x => x.Name == "Pasta");
        result.Recipes.Should().Contain(x => x.Name == "Pizza");
    }

    [Theory]
    [InlineData("Pas")]
    [InlineData("pas")]
    public async Task GetByQuery_CaseInsensitiveQuery_ReturnsFilteredRecipes(string query)
    {
        // Arrange
        var recipe1 = TestRecipes.Create("Pasta");
        RandomId.Set(recipe1);
        _recipes.Add(recipe1);
        
        var recipe2 = TestRecipes.Create("Pizza");
        RandomId.Set(recipe2);
        _recipes.Add(recipe2);

        // Act
        var result = await _sut.GetByQuery(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(1);
        result.Recipes.Should().Contain(x => x.Name == recipe1.Name);
    }
    
    [Fact]
    public async Task Get_MealDoesNotExist_ReturnsNull()
    {
        // Arrange
        const int nonExistingId = -15;
        
        // Act
        var result = await _sut.Get(nonExistingId, CancellationToken.None);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task Get_MealExists_ReturnsDetails()
    {
        // Arrange 
        var ingredient = Ingredient.Create("Bacon", [MeasureUnit.Kilogram]);
        var ingredients = AddIngredientAction.Create(ingredient, 1, MeasureUnit.Kilogram);
        var step = RecipeStep.Create(1, "Cook");
        var recipe = Recipe.Create("Burgers", 1, [ingredients], [step]);
        RandomId.Set(ingredient);
        RandomId.Set(step);
        RandomId.Set(recipe);
        RandomId.Set([.. recipe.Ingredients]);
        _recipes.Add(recipe);

        // Act
        var result = await _sut.Get(recipe.Id, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(recipe.Id);
        result.Name.Should().Be(recipe.Name);
        result.Servings.Should().Be(recipe.Servings);
        
        result.Ingredients.Should().HaveCount(1);
        var bacon = result.Ingredients.First();
        bacon.Id.Should().Be(ingredient.Id);
        bacon.Name.Should().Be(ingredient.Name);
        bacon.MeasureUnit.UnderlyingValue.Should().Be(nameof(MeasureUnit.Kilogram));
        bacon.Quantity.Should().Be(ingredients.Quantity);
        
        result.Steps.Should().HaveCount(1);
        var firstStep = result.Steps.First();
        firstStep.Id.Should().Be(step.Id);
        firstStep.Order.Should().Be(step.Order);
        firstStep.Instructions.Should().Be(step.Instructions);
    }
}
