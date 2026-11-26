using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MealCreatorTests
{
    private static readonly DateOnly Day = TestDates.Oct(14);
    private const int MissingRecipeId = 999;

    private readonly Recipe _soup = TestRecipes.CreateWithId(1, "Soup");
    private readonly Recipe _pasta = TestRecipes.CreateWithId(2, "Pasta");
    private readonly Recipe _salad = TestRecipes.CreateWithId(3, "Salad");
    private readonly Mock<MealPlannerDbContext> _ctx = new();
    private readonly List<Menu> _menus = [];
    private readonly MealCreator _sut;

    public MealCreatorTests()
    {
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>()))
            .Callback<Menu, CancellationToken>((menu, _) => _menus.Add(menu));
        _ctx.Setup(x => x.Recipes).ReturnsDbSet([_soup, _pasta, _salad]);
        _ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new MealCreator(_ctx.Object);
    }

    [Fact]
    public async Task Create_InsertsTheMealAtTheGivenPositionAndSaves()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_salad.Id, 2, 4), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(menu).Should().Equal(("Soup", 1, 2), ("Salad", 2, 4), ("Pasta", 3, 3));
        _menus.Should().ContainSingle();
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Create_AddsANewMenuAndSaves_WhenTheDayHasNone()
    {
        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_soup.Id, 1, 3), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var menu = _menus.Should().ContainSingle().Subject;
        menu.Date.Should().Be(Day);
        Describe(menu).Should().Equal(("Soup", 1, 3));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Create_Fails_WhenTheRecipeDoesNotExist()
    {
        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(MissingRecipeId, 1, 2), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Meal.MissingRecipesById(MissingRecipeId));
        _menus.Should().BeEmpty();
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Create_Fails_WhenTheRecipeIsAlreadyInTheDay()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_soup.Id, 3, 5), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay("Soup"));
        Describe(menu).Should().Equal(("Soup", 1, 2), ("Pasta", 2, 3));
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    public async Task Create_Fails_WhenThePositionIsOutOfRangeForTheDay(int position)
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_salad.Id, position, 4), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        Describe(menu).Should().Equal(("Soup", 1, 2), ("Pasta", 2, 3));
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Create_Fails_AndAddsNoMenu_WhenThePositionIsOutOfRangeForAnEmptyDay(int position)
    {
        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_soup.Id, position, 2), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().BeEmpty();
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_Fails_AndAddsNoMenu_WhenTheServingsAreNotPositive(int servings)
    {
        // Act
        var result = await _sut.Create(Day, new CreateMealRequest(_soup.Id, 1, servings), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().BeEmpty();
        VerifySaved(Times.Never());
    }

    [Theory]
    [MemberData(nameof(DaysOutsideTheSupportedRange))]
    public async Task Create_Fails_AndAddsNoMenu_WhenTheDayIsOutsideTheSupportedRange(DateOnly day)
    {
        // Act
        var result = await _sut.Create(day, new CreateMealRequest(_soup.Id, 1, 2), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().BeEmpty();
        VerifySaved(Times.Never());
    }

    public static TheoryData<DateOnly> DaysOutsideTheSupportedRange =>
        new()
        {
            Menu.MinDateInThePast.AddDays(-1),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1)
        };

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
