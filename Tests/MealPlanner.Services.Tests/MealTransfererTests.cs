using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Meals;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MealTransfererTests
{
    private static readonly DateOnly SourceDay = TestDates.Oct(14);
    private static readonly DateOnly TargetDay = TestDates.Oct(15);

    private readonly Recipe _soup = TestRecipes.CreateWithId(1, "Soup");
    private readonly Recipe _pasta = TestRecipes.CreateWithId(2, "Pasta");
    private readonly Recipe _salad = TestRecipes.CreateWithId(3, "Salad");
    private readonly Recipe _stew = TestRecipes.CreateWithId(4, "Stew");
    private readonly Recipe _bread = TestRecipes.CreateWithId(5, "Bread");
    private readonly Mock<MealPlannerDbContext> _ctx = new();
    private readonly List<Menu> _menus = [];
    private readonly MealTransferer _sut;

    public MealTransfererTests()
    {
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Recipes).ReturnsDbSet([_soup, _pasta, _salad, _stew, _bread]);
        _ctx.Setup(x => x.Menus.Add(It.IsAny<Menu>())).Callback<Menu>(menu => _menus.Add(menu));
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>()))
            .Callback<Menu, CancellationToken>((menu, _) => _menus.Add(menu));
        _ctx.Setup(x => x.Menus.Remove(It.IsAny<Menu>())).Callback<Menu>(menu => _menus.Remove(menu));
        _ctx.Setup(x => x.Add(It.IsAny<Menu>())).Callback<Menu>(menu => _menus.Add(menu));
        _ctx.Setup(x => x.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>()))
            .Callback<Menu, CancellationToken>((menu, _) => _menus.Add(menu));
        _ctx.Setup(x => x.Remove(It.IsAny<Menu>())).Callback<Menu>(menu => _menus.Remove(menu));
        _ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new MealTransferer(_ctx.Object);
    }

    [Fact]
    public async Task Transfer_Move_InsertsTheMealIntoTheTargetDayRemovesItFromTheSourceDayAndSavesOnce()
    {
        // Arrange
        var source = AddMenu(SourceDay, (_soup, 1, 2), (_stew, 2, 3), (_bread, 3, 4));
        var target = AddMenu(TargetDay, (_pasta, 1, 5), (_salad, 2, 6));

        // Act
        var result = await Transfer(SourceDay, _stew, TargetDay, 2, TransferMode.Move);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(source).Should().Equal(("Soup", 1, 2), ("Bread", 2, 4));
        Describe(target).Should().Equal(("Pasta", 1, 5), ("Stew", 2, 3), ("Salad", 3, 6));
        _menus.Should().HaveCount(2);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Transfer_Move_RemovesTheSourceMenuAndSavesOnce_WhenItWasTheLastMeal()
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));
        var target = AddMenu(TargetDay, (_pasta, 1, 5));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, 2, TransferMode.Move);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _menus.Should().ContainSingle().Which.Should().BeSameAs(target);
        Describe(target).Should().Equal(("Pasta", 1, 5), ("Soup", 2, 2));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Transfer_Move_AddsANewTargetMenuAndSavesOnce_WhenTheTargetDayHasNone()
    {
        // Arrange
        var source = AddMenu(SourceDay, (_soup, 1, 2), (_stew, 2, 3));

        // Act
        var result = await Transfer(SourceDay, _stew, TargetDay, 1, TransferMode.Move);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _menus.Should().HaveCount(2);
        Describe(source).Should().Equal(("Soup", 1, 2));
        Describe(MenuOn(TargetDay)).Should().Equal(("Stew", 1, 3));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Transfer_Move_ReplacesTheSourceMenuWithANewTargetMenu_WhenTheLastMealMovesToADayWithNoMenu()
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, 1, TransferMode.Move);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var target = _menus.Should().ContainSingle().Subject;
        target.Date.Should().Be(TargetDay);
        Describe(target).Should().Equal(("Soup", 1, 2));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Transfer_Copy_InsertsTheMealWithTheSameServingsKeepsTheSourceDayAndSavesOnce()
    {
        // Arrange
        var source = AddMenu(SourceDay, (_soup, 1, 2), (_stew, 2, 3));
        var target = AddMenu(TargetDay, (_pasta, 1, 5), (_salad, 2, 6));

        // Act
        var result = await Transfer(SourceDay, _stew, TargetDay, 2, TransferMode.Copy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Describe(source).Should().Equal(("Soup", 1, 2), ("Stew", 2, 3));
        Describe(target).Should().Equal(("Pasta", 1, 5), ("Stew", 2, 3), ("Salad", 3, 6));
        _menus.Should().HaveCount(2);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Transfer_Copy_KeepsTheSourceMenuAndAddsANewTargetMenu_WhenTheLastMealIsCopiedToADayWithNoMenu()
    {
        // Arrange
        var source = AddMenu(SourceDay, (_soup, 1, 3));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, 1, TransferMode.Copy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _menus.Should().HaveCount(2);
        Describe(source).Should().Equal(("Soup", 1, 3));
        Describe(MenuOn(TargetDay)).Should().Equal(("Soup", 1, 3));
        VerifySaved(Times.Once());
    }

    [Theory]
    [InlineData(TransferMode.Move)]
    [InlineData(TransferMode.Copy)]
    public async Task Transfer_Fails_WhenTheTargetDayIsTheSourceDay(TransferMode mode)
    {
        // Arrange
        var menu = AddMenu(SourceDay, (_soup, 1, 2), (_pasta, 2, 3));

        // Act
        var result = await Transfer(SourceDay, _soup, SourceDay, 2, mode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Meal.TransferToTheSameDay);
        Describe(menu).Should().Equal(("Soup", 1, 2), ("Pasta", 2, 3));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Transfer_Fails_WhenTheSourceDayHasNoMenu()
    {
        // Arrange
        AddMenu(TargetDay, (_pasta, 1, 5));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, 1, TransferMode.Move);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Menu.DoesNotExist(SourceDay));
        _menus.Should().ContainSingle();
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Transfer_Fails_AndAddsNoMenu_WhenTheMealIsNotInTheSourceDay()
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));

        // Act
        var result = await Transfer(SourceDay, _pasta, TargetDay, 1, TransferMode.Move);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(_pasta.Id));
        _menus.Should().ContainSingle().Which.Date.Should().Be(SourceDay);
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(TransferMode.Move)]
    [InlineData(TransferMode.Copy)]
    public async Task Transfer_Fails_AndKeepsBothMenus_WhenTheRecipeIsAlreadyInTheTargetDay(TransferMode mode)
    {
        // Arrange
        AddMenu(SourceDay, (_stew, 1, 3));
        var target = AddMenu(TargetDay, (_pasta, 1, 5), (_stew, 2, 6));

        // Act
        var result = await Transfer(SourceDay, _stew, TargetDay, 1, mode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay("Stew"));
        _menus.Should().HaveCount(2);
        Describe(target).Should().Equal(("Pasta", 1, 5), ("Stew", 2, 6));
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(TransferMode.Move, 0)]
    [InlineData(TransferMode.Move, -1)]
    [InlineData(TransferMode.Move, 4)]
    [InlineData(TransferMode.Copy, 0)]
    [InlineData(TransferMode.Copy, -1)]
    [InlineData(TransferMode.Copy, 4)]
    public async Task Transfer_Fails_AndKeepsBothMenus_WhenThePositionIsOutOfRangeForTheTargetDay(TransferMode mode, int position)
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));
        var target = AddMenu(TargetDay, (_pasta, 1, 5), (_salad, 2, 6));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, position, mode);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().HaveCount(2);
        Describe(target).Should().Equal(("Pasta", 1, 5), ("Salad", 2, 6));
        VerifySaved(Times.Never());
    }

    [Theory]
    [InlineData(TransferMode.Move, 0)]
    [InlineData(TransferMode.Move, -1)]
    [InlineData(TransferMode.Move, 2)]
    [InlineData(TransferMode.Copy, 0)]
    [InlineData(TransferMode.Copy, -1)]
    [InlineData(TransferMode.Copy, 2)]
    public async Task Transfer_Fails_AndAddsNoMenu_WhenThePositionIsOutOfRangeForATargetDayWithNoMenu(TransferMode mode, int position)
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));

        // Act
        var result = await Transfer(SourceDay, _soup, TargetDay, position, mode);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().ContainSingle().Which.Date.Should().Be(SourceDay);
        VerifySaved(Times.Never());
    }

    [Theory]
    [MemberData(nameof(DaysOutsideTheSupportedRange))]
    public async Task Transfer_Fails_AndAddsNoMenu_WhenTheTargetDayIsOutsideTheSupportedRange(DateOnly targetDay)
    {
        // Arrange
        AddMenu(SourceDay, (_soup, 1, 2));

        // Act
        var result = await Transfer(SourceDay, _soup, targetDay, 1, TransferMode.Move);

        // Assert
        result.IsFailure.Should().BeTrue();
        _menus.Should().ContainSingle().Which.Date.Should().Be(SourceDay);
        VerifySaved(Times.Never());
    }

    public static TheoryData<DateOnly> DaysOutsideTheSupportedRange =>
        new()
        {
            Menu.MinDateInThePast.AddDays(-1),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1)
        };

    private Task<Result> Transfer(DateOnly day, Recipe recipe, DateOnly targetDay, int position, TransferMode mode) =>
        _sut.Transfer(day, recipe.Id, new TransferMealRequest(targetDay, position, mode), CancellationToken.None);

    private Menu AddMenu(DateOnly day, params (Recipe Recipe, int Order, int Servings)[] meals)
    {
        var menu = TestMenu.Create(day, meals.Select(meal => TestActions.AddMeal(meal.Recipe, meal.Order, meal.Servings)).ToList());
        _menus.Add(menu);
        return menu;
    }

    private Menu MenuOn(DateOnly day) => _menus.Single(menu => menu.Date == day);

    private static List<(string Name, int Order, int Servings)> Describe(Menu menu) =>
        menu.Meals.OrderBy(meal => meal.Order).Select(meal => (meal.Recipe.Name, meal.Order, meal.Servings)).ToList();

    private void VerifySaved(Times times) => _ctx.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
}
