using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Meals;
using MealPlanner.Services.Menus;
using MealPlanner.Shared.Menus.Requests;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuCreatorTests
{
    private static readonly Meal PreExistingMeal = Meal.Create("Fish and chips");
    private static readonly Dictionary<int, string> Meals = new()
    {
        { 1, PreExistingMeal.Name }
    };

    private readonly MenuCreator _sut;

    private readonly List<Menu> _menus = [];
    private readonly List<Meal> _meals = [PreExistingMeal];
    
    public MenuCreatorTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
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

        ctx.Setup(x => x.Meals).ReturnsDbSet(_meals);
        
        ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        IMapMeals mealsMapper = new MealMapper(ctx.Object);
        _sut = new MenuCreator(ctx.Object, mealsMapper);
    }
    
    [Fact]
    public async Task Create_CreatesSuccessfully_ReturnsId()
    {
        // Arrange
        CreateMenuRequest request = new(DateOnly.FromDateTime(DateTime.Today), Meals);
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Date.Should().Be(request.Date);
    }
    
    [Fact]
    public async Task Create_ThrowsWhenMealsAreEmpty()
    {
        // Arrange
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), new Dictionary<int, string>());
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No meals were provided.");
    }
    
    // TODO: move it to Meal mapper tests
    [Fact]
    public async Task Create_ThrowsWhenMealDoesNotExist()
    {
        // Arrange
        var newMeal = Meal.Create("Quesadilla");
        var updatedMeals = new Dictionary<int, string>(Meals) { { 2, newMeal.Name } };
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), updatedMeals);
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Meal with name {newMeal.Name} does not exist");
        
        // Cleanup
        _meals.Remove(newMeal);
    }

    [Fact]
    public async Task Create_ThrowsWhenMenuAlreadyPresentForSpecifiedDay()
    {
        // Arrange
        var tomorrow =  DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var request = new CreateMenuRequest(tomorrow, Meals);
        await _sut.Create(request, CancellationToken.None);
        var mealsForConflictingRequest = new Dictionary<int, string>() { { 0, "Pierogi" } };
        var conflictingRequest = new CreateMenuRequest(tomorrow, mealsForConflictingRequest);

        // Act
        var createWithConflict = async (CreateMenuRequest req) => await _sut.Create(req, CancellationToken.None);
        
        // Assert
        await createWithConflict.Awaiting(x => x.Invoke(conflictingRequest))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"There is already a Menu defined for {conflictingRequest.Date}.");
    }
}