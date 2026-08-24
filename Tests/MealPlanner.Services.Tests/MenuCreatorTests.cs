using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuCreatorTests
{
    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Fish and chips");
    private static readonly List<AddMealRequest> Meals = [new AddMealRequest(PreExistingRecipe.Id, 1, 1)];

    private readonly MenuCreator _sut;

    private readonly List<Menu> _menus = [];
    private readonly List<Recipe> _recipes = [PreExistingRecipe];
    
    public MenuCreatorTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        ctx.Setup(x => x.Menus.AddAsync(It.IsAny<Menu>(), It.IsAny<CancellationToken>())).Callback<Menu, CancellationToken>((menu, _) =>
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

        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        
        ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        IMapMeals mealsMapper = new MealsMapper(ctx.Object);
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
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Date.Should().Be(request.Date);
    }
    
    [Fact]
    public async Task Create_Fails_WhenMealsAreEmpty()
    {
        // Arrange
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), []);
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Menu.InvalidMeals);
    }
    
    // TODO: move it to Meal mapper tests
    [Fact]
    public async Task Create_Fails_WhenMealDoesNotExist()
    {
        // Arrange
        const int missingRecipeId = 999;
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), [new AddMealRequest(missingRecipeId, 2, 1)]);
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Meal.MissingRecipesById(missingRecipeId));
    }

    [Fact]
    public async Task Create_Fails_WhenMenuAlreadyPresentForSpecifiedDay()
    {
        // Arrange
        var tomorrow =  DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var request = new CreateMenuRequest(tomorrow, Meals);
        await _sut.Create(request, CancellationToken.None);
        var recipeForConflictingRequest = TestRecipes.Create("Pierogi");
        List<AddMealRequest> mealsForConflictingRequest = [new(recipeForConflictingRequest.Id, 2, 1)];
        var conflictingRequest = new CreateMenuRequest(tomorrow, mealsForConflictingRequest);

        // Act
        var result = await _sut.Create(conflictingRequest, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Menu.AlreadyExists(conflictingRequest.Date));
    }
}