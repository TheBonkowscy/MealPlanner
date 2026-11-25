using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Services.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MealDeleterTests
{
    private static readonly DateOnly Day = TestDates.Oct(14);

    private readonly Recipe _soup = TestRecipes.CreateWithId(1, "Soup");
    private readonly Recipe _pasta = TestRecipes.CreateWithId(2, "Pasta");
    private readonly Recipe _salad = TestRecipes.CreateWithId(3, "Salad");
    private readonly Mock<MealPlannerDbContext> _ctx = new();
    private readonly List<Menu> _menus = [];
    private readonly MealDeleter _sut;

    public MealDeleterTests()
    {
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.Remove(It.IsAny<Menu>())).Callback<Menu>(menu => _menus.Remove(menu));
        _ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new MealDeleter(_ctx.Object);
    }

    [Fact]
    public async Task Delete_RemovesTheMealAndSaves()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2), (_pasta, 2, 3), (_salad, 3, 4));

        // Act
        var result = await _sut.Delete(Day, _pasta.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(menu).Should().Equal(("Soup", 1, 2), ("Salad", 2, 4));
        _menus.Should().ContainSingle();
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Delete_RemovesTheMenuAndSaves_WhenItWasTheLastMeal()
    {
        // Arrange
        AddMenu((_soup, 1, 2));

        // Act
        var result = await _sut.Delete(Day, _soup.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _menus.Should().BeEmpty();
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Delete_Fails_WhenTheDayHasNoMenu()
    {
        // Act
        var result = await _sut.Delete(Day, _soup.Id, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Menu.DoesNotExist(Day));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Delete_Fails_WhenTheMealIsNotInTheDay()
    {
        // Arrange
        var menu = AddMenu((_soup, 1, 2));

        // Act
        var result = await _sut.Delete(Day, _pasta.Id, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(_pasta.Id));
        _menus.Should().ContainSingle();
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
