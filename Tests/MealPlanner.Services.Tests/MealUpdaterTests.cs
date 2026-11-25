using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MealUpdaterTests
{
    private static readonly DateOnly Day = TestDates.Oct(14);

    private readonly Recipe _soup = TestRecipes.CreateWithId(1, "Soup");
    private readonly Recipe _pasta = TestRecipes.CreateWithId(2, "Pasta");
    private readonly Recipe _salad = TestRecipes.CreateWithId(3, "Salad");
    private readonly Mock<MealPlannerDbContext> _ctx = new();
    private readonly List<Menu> _menus = [];
    private readonly MealUpdater _sut;

    public MealUpdaterTests()
    {
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new MealUpdater(_ctx.Object);
    }

    [Fact]
    public async Task Update_ChangesTheServingsAndSaves()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Update(Day, _soup.Id, new UpdateMealRequest(Servings: 5, Position: null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(menu).Should().Equal(("Soup", 1, 5), ("Pasta", 2, 3));
        VerifySaved(Times.Once());
    }

    [Theory]
    [MemberData(nameof(MovesWithResultingOrder))]
    public async Task Update_MovesTheMealAndSaves(string movedMeal, int position, string[] expectedOrder)
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3), (_salad, 3, 4));
        var moved = new[] { _soup, _pasta, _salad }.Single(recipe => recipe.Name == movedMeal);

        // Act
        var result = await _sut.Update(Day, moved.Id, new UpdateMealRequest(Servings: null, Position: position), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var meals = Describe(menu);
        meals.Select(meal => meal.Name).Should().Equal(expectedOrder);
        meals.Select(meal => meal.Order).Should().Equal(1, 2, 3);
        meals.ToDictionary(meal => meal.Name, meal => meal.Servings)
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["Soup"] = 2, ["Pasta"] = 3, ["Salad"] = 4 });
        VerifySaved(Times.Once());
    }

    public static TheoryData<string, int, string[]> MovesWithResultingOrder =>
        new()
        {
            { "Soup", 3, ["Pasta", "Salad", "Soup"] },
            { "Salad", 1, ["Salad", "Soup", "Pasta"] },
            { "Pasta", 3, ["Soup", "Salad", "Pasta"] },
            { "Pasta", 2, ["Soup", "Pasta", "Salad"] }
        };

    [Fact]
    public async Task Update_ChangesTheServingsAndThePositionAndSaves()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Update(Day, _pasta.Id, new UpdateMealRequest(Servings: 6, Position: 1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(menu).Should().Equal(("Pasta", 1, 6), ("Soup", 2, 2));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Update_Fails_WhenThereIsNothingToChange()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2));

        // Act
        var result = await _sut.Update(Day, _soup.Id, new UpdateMealRequest(Servings: null, Position: null), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
        Describe(menu).Should().Equal(("Soup", 1, 2));
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(null, -1)]
    [InlineData(null, 3)]
    [InlineData(0, null)]
    [InlineData(-2, null)]
    [InlineData(9, 3)]
    [InlineData(0, 2)]
    public async Task Update_Fails_AndDoesNotSave_WhenAChangeIsInvalid(int? servings, int? position)
    {
        // Arrange
        AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Update(Day, _soup.Id, new UpdateMealRequest(servings, position), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Update_Fails_WhenTheDayHasNoMenu()
    {
        // Act
        var result = await _sut.Update(Day, _soup.Id, new UpdateMealRequest(Servings: 5, Position: null), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Menu.DoesNotExist(Day));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Update_Fails_WhenTheMealIsNotInTheDay()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2));

        // Act
        var result = await _sut.Update(Day, _pasta.Id, new UpdateMealRequest(Servings: 5, Position: null), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(_pasta.Id));
        Describe(menu).Should().Equal(("Soup", 1, 2));
        VerifySaved(Times.Never());
    }

    private Menu AddMenu(params (Recipe Recipe, int Order, int Servings)[] meals)
    {
        var menu = TestMenu.Create(Day, meals.Select(meal => TestActions.AddMeal(meal.Recipe, meal.Order, meal.Servings)).ToList());
        _menus.Add(menu);
        return menu;
    }

    private static List<(string Name, int Order, int Servings)> Describe(Menu menu) =>
        menu.Meals.OrderBy(meal => meal.Order).Select(meal => (meal.Recipe.Name, meal.Order, meal.Servings)).ToList();

    private void VerifySaved(Times times) => _ctx.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
}
