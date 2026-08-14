using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class MenuCreatorTests
{
    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Fish and chips");
    private static readonly Dictionary<int, string> Meals = new()
    {
        { 1, PreExistingRecipe.Name }
    };

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
        IMapRecipe recipeMapper = new RecipeMapper(ctx.Object);
        _sut = new MenuCreator(ctx.Object, recipeMapper);
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
        var newRecipe = TestRecipes.Create("Quesadilla");
        var updatedMeals = new Dictionary<int, string>(Meals) { { 2, newRecipe.Name } };
        var request = new CreateMenuRequest(DateOnly.FromDateTime(DateTime.Today), updatedMeals);
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Recipe for {newRecipe.Name} does not exist");
        
        // Cleanup
        _recipes.Remove(newRecipe);
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