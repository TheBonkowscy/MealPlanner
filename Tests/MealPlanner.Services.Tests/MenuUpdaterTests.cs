using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Meals;
using MealPlanner.Services.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuUpdaterTests
{
    private static readonly Meal PreExistingMeal = Meal.Create("Fish and chips");

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly Mock<IMapMeals> _mealsMapper;
    private readonly MenuUpdater _sut;

    private readonly List<Menu> _menus = [];
    private readonly List<Meal> _meals = [PreExistingMeal];
    

    public MenuUpdaterTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet([]);
        
        _mealsMapper = new Mock<IMapMeals>();
        _ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        _ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
        {
            RandomId.Set(menu);
            _menus.Add(menu);

            menu.Items.ToList().ForEach(meal =>
            {
                if (_meals.All(x => x.Name != meal.Meal.Name))
                {
                    _meals.Add(meal.Meal);
                }
            });
        });

        _ctx.Setup(x => x.Meals).ReturnsDbSet(_meals);
        
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
        var menuForDate = Menu.Create(date, [PreExistingMeal]);
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
        var menuForDate = Menu.Create(date, [PreExistingMeal]);
        _menus.Add(menuForDate);
        var mealDtos = new Dictionary<int, string>() { { 0, PreExistingMeal.Name } };

        _mealsMapper
            .Setup(x => x.MapMeals(mealDtos, It.IsAny<CancellationToken>()))
            .ReturnsAsync([PreExistingMeal]);

        var request = new UpdateMenuRequest(date, mealDtos);

        // Act
        var result = await _sut.Update(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);

        _mealsMapper.Verify(x => x.MapMeals(mealDtos, It.IsAny<CancellationToken>()), Times.Once);
    }
}