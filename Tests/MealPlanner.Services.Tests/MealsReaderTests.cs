using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Meals.Read;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MealsReaderTests
{
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly MealsReader _sut;
    private readonly List<Meal> _meals = [];

    public MealsReaderTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Meals).ReturnsDbSet(_meals);
        _sut = new MealsReader(_ctx.Object);
    }

    [Fact]
    public async Task GetByQuery_NoQuery_ReturnsAllMeals()
    {
        // Arrange
        var meal1 = Meal.Create("Pasta");
        RandomId.Set(meal1);
        _meals.Add(meal1);
        
        var meal2 = Meal.Create("Pizza");
        RandomId.Set(meal2);
        _meals.Add(meal2);

        // Act
        var result = await _sut.GetByQuery(null, CancellationToken.None);

        // Assert
        result.Meals.Should().HaveCount(2);
        result.Meals.Should().Contain(x => x.Name == "Pasta");
        result.Meals.Should().Contain(x => x.Name == "Pizza");
    }

    [Theory]
    [InlineData("Pas")]
    [InlineData("pas")]
    public async Task GetByQuery_CaseInsensitiveQuery_ReturnsFilteredMeals(string query)
    {
        // Arrange
        var meal1 = Meal.Create("Pasta");
        RandomId.Set(meal1);
        _meals.Add(meal1);
        
        var meal2 = Meal.Create("Pizza");
        RandomId.Set(meal2);
        _meals.Add(meal2);

        // Act
        var result = await _sut.GetByQuery(query, CancellationToken.None);

        // Assert
        result.Meals.Should().HaveCount(1);
        result.Meals.Should().Contain(x => x.Name == "Pasta");
    }
}
