using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuUpdaterTests
{
    private static readonly Recipe PreExistingRecipe = Recipe.Create("Fish and chips");

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly Mock<IMapRecipes> _mealsMapper;
    private readonly MenuUpdater _sut;

    private readonly List<Menu> _menus = [];
    private readonly List<Recipe> _recipes = [PreExistingRecipe];
    

    public MenuUpdaterTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet([]);
        
        _mealsMapper = new Mock<IMapRecipes>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
        {
            RandomId.Set(menu);
            _menus.Add(menu);

            menu.Meals.ToList().ForEach(meal =>
            {
                if (_recipes.All(x => x.Name != meal.Recipe.Name))
                {
                    _recipes.Add(meal.Recipe);
                }
            });
        });

        _ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        
        _ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _sut = new MenuUpdater(_ctx.Object, _mealsMapper.Object);
    }

    [Fact]
    public async Task Update_WhenMenuDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new UpdateMenuRequest(new DateOnly(2026, 8, 6),new Dictionary<int, string>());

        // Act
        var result = async () => await _sut.Update(request, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Menu for {request.Date} does not exist.");

        _ctx.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenMealsListIsEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 6);
        var menuForDate = Menu.Create(date, [PreExistingRecipe]);
        _menus.Add(menuForDate);
        var request = new UpdateMenuRequest(date, new Dictionary<int, string>());

        // Act
        Func<Task> act = async () => await _sut.Update(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No meals were provided.");
        
        _ctx.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        // Cleanup
        _menus.Remove(menuForDate);
    }

    [Fact]
    public async Task Update_WhenValidRequest_ShouldCallMapperAndSaveChanges()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 6);
        var menuForDate = Menu.Create(date, [PreExistingRecipe]);
        _menus.Add(menuForDate);
        var mealDtos = new Dictionary<int, string>() { { 0, PreExistingRecipe.Name } };

        _mealsMapper
            .Setup(x => x.MapRecipes(mealDtos, It.IsAny<CancellationToken>()))
            .ReturnsAsync([PreExistingRecipe]);

        var request = new UpdateMenuRequest(date, mealDtos);

        // Act
        var result = await _sut.Update(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);

        _mealsMapper.Verify(x => x.MapRecipes(mealDtos, It.IsAny<CancellationToken>()), Times.Once);
    }
}