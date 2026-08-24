using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Shared;
using MealPlanner.Tests.Shared;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class IngredientsReaderTests
{
    private readonly Mock<IStringLocalizer<Translations>> _localiser;
    private readonly IngredientReader _sut;
    private readonly List<Ingredient> _ingredients = [];

    public IngredientsReaderTests()
    {
        _localiser = new Mock<IStringLocalizer<Translations>>();
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        _sut = new IngredientReader(ctx.Object, new MeasureUnitMapper(_localiser.Object));
    }

    [Fact]
    public async Task Get_ReturnsAllIngredients()
    {
        // Arrange
        const string flourName = "Flour";
        var flour = Ingredient.Create(flourName, [MeasureUnit.Gram, MeasureUnit.Tablespoon, MeasureUnit.GlassCup]).Value;
        const string milkName = "Milk";
        var milk = Ingredient.Create(milkName, [MeasureUnit.Liter, MeasureUnit.Milliliter]).Value;
        RandomId.Set(flour, milk);
        _ingredients.AddRange(flour, milk);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        result.Ingredients.Should().HaveCount(2);
        var flourResponse = result.Ingredients.First(x => x.Name == flourName);
        var milkResponse = result.Ingredients.First(x => x.Name == milkName);
        
        flourResponse.ApplicableUnits.Should().BeEquivalentTo(ToResponse(flour.ApplicableUnits));
        milkResponse.ApplicableUnits.Should().BeEquivalentTo(ToResponse(milk.ApplicableUnits));
    }


    private IEnumerable<MeasureUnitDto> ToResponse(IEnumerable<MeasureUnit> unitsToConvert) =>
    [
        .. unitsToConvert.Select(x => new MeasureUnitDto(_localiser.Object.GetString(x.ToString()), x.ToString()))
    ];
}
