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
    private const string MealName = "Fish and chips";
    private static readonly Meal PreExistingMeal = Meal.Create("Pizza");

    private readonly Mock<MealPlannerDbContext> _ctx;
    private readonly IMapMeals _mealsMapper;
    private readonly MenuCreator _sut;

    private List<Menu> _menus = [];
    private List<Meal> _meals = [PreExistingMeal];
    
    public MenuCreatorTests()
    {
        _ctx = new Mock<MealPlannerDbContext>();
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
        _mealsMapper = new MealMapper(_ctx.Object);
        _sut = new MenuCreator(_ctx.Object, _mealsMapper);
    }
    
    [Theory]
    [MemberData(nameof(ValidCreateRequests))]
    public async Task Create_CreatesSuccessfully_ReturnsId(CreateMenuRequest createMenuRequest)
    {
        // Act
        var result = await _sut.Create(createMenuRequest, CancellationToken.None);
        
        // Assert
        result.Date.Should().Be(createMenuRequest.Date);
    }
    
    [Fact]
    public async Task Create_CreatesOnlyNewMeals_ReturnsId()
    {
        // Arrange
        var newMeal = "Quesadilla";
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), 
            [PreExistingMeal.Name, newMeal]);
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Date.Should().Be(result.Date);
        _meals.Count.Should().Be(2);
        _meals.Should().Contain(x => x.Name.Equals(newMeal, StringComparison.CurrentCultureIgnoreCase));
    }

    [Fact]
    public async Task Create_ThrowsWhenMenuAlreadyPresentForSpecifiedDay()
    {
        // Arrange
        var tomorrow =  DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var request = new CreateMenuRequest(tomorrow, [MealName]);
        await _sut.Create(request, CancellationToken.None);
        var conflictingRequest = new CreateMenuRequest(tomorrow, ["Pierogi"]);

        // Act
        var createWithConflict = async (CreateMenuRequest req) => await _sut.Create(req, CancellationToken.None);
        
        // Assert
        await createWithConflict.Awaiting(x => x.Invoke(conflictingRequest))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"There is already a Menu defined for {conflictingRequest.Date}.");
    }

    public static TheoryData<CreateMenuRequest> ValidCreateRequests
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new TheoryData<CreateMenuRequest>
            {
                new(today, [MealName]),
                new(today.AddDays(1), []),
                new(today.AddDays(2))
            };

            return data;
        }
    }
}