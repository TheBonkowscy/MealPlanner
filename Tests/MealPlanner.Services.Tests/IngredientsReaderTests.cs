using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Shared.Shared;
using MealPlanner.Tests.Shared;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class IngredientsReaderTests
{
    private readonly Mock<IStringLocalizer<MeasureUnitMapper>> _localiser;
    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly IngredientReader _sut;
    private readonly List<Ingredient> _ingredients = [];

    public IngredientsReaderTests()
    {
        _localiser = new Mock<IStringLocalizer<MeasureUnitMapper>>();
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        _sut = new IngredientReader(_ctx.Object, new MeasureUnitMapper(_localiser.Object));
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
        
        flourResponse.ApplicableUnits.Should().BeEquivalentTo(ToResponse(ingredient1.ApplicableUnits));
        milkResponse.ApplicableUnits.Should().BeEquivalentTo(ToResponse(ingredient2.ApplicableUnits));
    }


    private IEnumerable<MeasureUnitDto> ToResponse(IEnumerable<MeasureUnit> unitsToConvert) =>
    [
        .. unitsToConvert.Select(x => new MeasureUnitDto(_localiser.Object.GetString(x.ToString()), x.ToString()))
    ];
}
