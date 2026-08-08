using AwesomeAssertions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Recipe SharedFirstRecipe = Recipe.Create("Fish and chips");
    private static readonly Recipe SharedSecondRecipe = Recipe.Create("Pierogi");
    private static readonly string InvalidDateExceptionMessage = $"Invalid date specified. The date can not be before {Menu.MinDateInThePast} and must be in the near future.";

    [Theory]
    [MemberData(nameof(InvalidDatesSource))]
    public void Create_ThrowsForInvalidDate(DateOnly invalidDate)
    {
        // Act
        Action<DateOnly> createNewMenu = date => TestMenu.Create(date);
        
        // Assert
        createNewMenu.Invoking(x => x.Invoke(invalidDate))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(InvalidDateExceptionMessage);
    }

    [Theory]
    [MemberData(nameof(ValidDatesSource))]
    public void Create_CreatesSuccessfully(DateOnly validDate)
    {
        // Act
        var result = TestMenu.Create(validDate);
        
        // Assert
        result.Date.Should().Be(validDate);
    }

    [Fact]
    public void AddMeal_WithoutOrder_SuccessfullyAddsMeal()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        
        // Act
        menu.AddMeal(SharedSecondRecipe);
        
        // Assert
        menu.Meals.Should().HaveCount(2);
        menu.GetMeal(0).Should().Be(SharedFirstRecipe);
        menu.GetMeal(1).Should().Be(SharedSecondRecipe);
    }

    [Fact]
    public void AddMeal_WithoutOrder_KeepsOrder()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        
        // Act
        menu.AddMeal(SharedSecondRecipe);
        
        // Assert
        menu.Meals.Should().HaveCount(2);
        menu.GetMeal(0).Should().Be(SharedFirstRecipe);
        menu.GetMeal(1).Should().Be(SharedSecondRecipe);
    }

    [Fact]
    public void AddMeal_WithMealAlreadyAdded_ThrowsException()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        
        // Act
        Action<Recipe> addMealToMenu = menu.AddMeal;
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(SharedFirstRecipe))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Meal '{SharedFirstRecipe}' is already present in the menu for {menu.Date}.");
    }

    [Fact]
    public void AddMeal_WithOrder_SuccessfullyAddsMeal()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        const int order = 1;
        
        // Act
        menu.AddMeal(order, SharedSecondRecipe);
        
        // Assert
        menu.Meals.Should().HaveCount(2);
        menu.GetMeal(order)!.Name.Should().Be(SharedSecondRecipe.Name);
    }

    [Fact]
    public void AddMeal_WithOrder_WhenOrderAlreadyTaken_ThrowsException()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        const int mealOrder = 0;
        
        // Act
        Action<int, Recipe> addMealToMenu = menu.AddMeal;
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(mealOrder, SharedSecondRecipe))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"There is already a meal added as #{mealOrder + 1} in the day");
    }

    [Fact]
    public void AddMeal_WithOrder_WithMealAlreadyAdded_ThrowsException()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate, [SharedFirstRecipe]);
        
        // Act
        Action<int, Recipe> addMealToMenu = menu.AddMeal;
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(1, SharedFirstRecipe))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Meal '{SharedFirstRecipe}' is already present in the menu for {menu.Date}.");
    }

    [Fact]
    public void AddMeal_WithOrder_ThrowsExceptionForNegativeOrder()
    {
        // Arrange
        var menu = TestMenu.Create(SharedDate);
        
        // Act
        Action<int, Recipe> addMealToMenu = menu.AddMeal;
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(-1, SharedFirstRecipe))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Order must be a positive number.");
    }

    public static TheoryData<DateOnly> InvalidDatesSource
    {
        get
        {
            var data = new TheoryData<DateOnly>
            {
                DateOnly.MinValue,
                DateOnly.MaxValue, 
                Menu.MinDateInThePast.AddDays(-1),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1)
                
            };
            return data;
        }
    }

    public static TheoryData<DateOnly> ValidDatesSource
    {
        get
        {
            var data = new TheoryData<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow),
                Menu.MinDateInThePast,
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100)
                
            };
            return data;
        }
    }
}