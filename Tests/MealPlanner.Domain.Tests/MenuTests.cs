using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Recipe SharedFirstRecipe = TestRecipes.Create("Fish and chips");
    private static readonly Recipe SharedSecondRecipe = TestRecipes.Create("Pierogi");
    private static readonly string InvalidDateExceptionMessage = $"Invalid date specified. The date can not be before {Menu.MinDateInThePast} and must be in the near future.";

    [Theory]
    [MemberData(nameof(InvalidDatesSource))]
    public void Create_ThrowsForInvalidDate(DateOnly invalidDate)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1), AddMealAction.Create(SharedSecondRecipe, 2, 1)];
            
        // Act
        Action<DateOnly> createNewMenu = date => Menu.Create(date, mealsToAdd);
        
        // Assert
        createNewMenu.Invoking(x => x.Invoke(invalidDate))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(InvalidDateExceptionMessage);
    }

    [Theory]
    [MemberData(nameof(ValidDatesSource))]
    public void Create_CreatesSuccessfully(DateOnly validDate)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1), AddMealAction.Create(SharedSecondRecipe, 2, 1)];
        
        // Act
        var result = Menu.Create(validDate, mealsToAdd);
        
        // Assert
        result.Date.Should().Be(validDate);
    }

    [Fact]
    public void AddMeal_SuccessfullyAddsMeal_KeepsOrder()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1)];
        var menu = Menu.Create(SharedDate, mealsToAdd);
        
        // Act
        menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 2, 1));
        
        // Assert
        menu.Meals.Should().HaveCount(2);
        menu.GetRecipe(1).Should().Be(SharedFirstRecipe);
        menu.GetRecipe(2).Should().Be(SharedSecondRecipe);
    }

    [Fact]
    public void AddMeal_Throws_WhenOrderAlreadyTaken()
    {
        // Arrange
        const int order = 1;
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, order, 1), ];
        var menu = Menu.Create(SharedDate, mealsToAdd);
        
        // Act
        var addMeal = () => menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, order, 1));
        
        // Assert
        addMeal.Should().Throw<InvalidOperationException>()
            .WithMessage($"There is already a meal added as #{order + 1} in the day");
    }

    [Fact]
    public void AddMeal_WithMealAlreadyAdded_ThrowsException()
    {
        // Arrange
        var firstMeal = AddMealAction.Create(SharedFirstRecipe, 1, 1);
        List<AddMealAction> mealsToAdd = [firstMeal, AddMealAction.Create(SharedSecondRecipe, 2, 1)];
        var menu = Menu.Create(SharedDate, mealsToAdd);
        var thirdMeal = AddMealAction.Create(SharedFirstRecipe, 3, 1);
        
        // Act
        var addMealToMenu = menu.AddMeal;
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(thirdMeal))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Meal '{SharedFirstRecipe.Name}' is already present in the menu for {menu.Date}.");
    }

    [Fact]
    public void AddMeal_Throws_WhenOrderIsOutOfBounds()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1), ];
        var menu = Menu.Create(SharedDate, mealsToAdd);
        
        // Act
        var addMeal = () => menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 999, 1));
        
        // Assert
        addMeal.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Order must not exceed the number of already added meals.");
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