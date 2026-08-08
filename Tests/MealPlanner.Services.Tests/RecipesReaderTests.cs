using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Meals.Read;
using MealPlanner.Tests.Shared;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class RecipesReaderTests
{
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly RecipesReader _sut;
    private readonly List<Recipe> _recipes = [];

    public RecipesReaderTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        _sut = new RecipesReader(_ctx.Object);
    }

    [Fact]
    public async Task GetByQuery_NoQuery_ReturnsAllMeals()
    {
        // Arrange
        var meal1 = Recipe.Create("Pasta");
        RandomId.Set(meal1);
        _recipes.Add(meal1);
        
        var meal2 = Recipe.Create("Pizza");
        RandomId.Set(meal2);
        _recipes.Add(meal2);

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
    public async Task GetByQuery_CaseInsensitiveQuery_ReturnsFilteredMeals(string query)
    {
        // Arrange
        var meal1 = Recipe.Create("Pasta");
        RandomId.Set(meal1);
        _recipes.Add(meal1);
        
        var meal2 = Recipe.Create("Pizza");
        RandomId.Set(meal2);
        _recipes.Add(meal2);

        // Act
        var result = await _sut.GetByQuery(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(1);
        result.Recipes.Should().Contain(x => x.Name == "Pasta");
    }
}
