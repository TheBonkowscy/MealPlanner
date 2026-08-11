using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Ingredients;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Tests.Shared;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class IngredientsReaderTests
{
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly IngredientReader _sut;
    private readonly List<Ingredient> _ingredients = [];

    public IngredientsReaderTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        _sut = new IngredientReader(_ctx.Object);
    }

    [Fact]
    public async Task Get_ReturnsAllIngredients()
    {
        // Arrange
        const string flour = "Flour";
        var ingredient1 = Ingredient.Create(flour, [MeasureUnit.Gram, MeasureUnit.Tablespoon, MeasureUnit.GlassCup]);
        const string milk = "Milk";
        var ingredient2 = Ingredient.Create(milk, [MeasureUnit.Liter, MeasureUnit.Milliliter]);
        RandomId.Set(ingredient1, ingredient2);
        _ingredients.AddRange(ingredient1, ingredient2);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        result.Ingredients.Should().HaveCount(2);
        var flourResponse = result.Ingredients.First(x => x.Name == flour);
        var milkResponse = result.Ingredients.First(x => x.Name == milk);
        
        flourResponse.Units.Should().BeEquivalentTo(ToResponse(ingredient1.ApplicableUnits));
        milkResponse.Units.Should().BeEquivalentTo(ToResponse(ingredient2.ApplicableUnits));
    }


    private static IEnumerable<IngredientMeasureUnitsResponse> ToResponse(IEnumerable<MeasureUnit> unitsToConvert) =>
    [
        .. unitsToConvert.Select(x => new IngredientMeasureUnitsResponse(x.ToString(), x.ToString()))
    ];
}
