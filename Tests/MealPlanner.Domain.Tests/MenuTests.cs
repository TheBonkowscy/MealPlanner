using AwesomeAssertions;
using AwesomeAssertions.Execution;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Menus.Exceptions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Recipe SharedFirstRecipe = TestRecipes.Create("Fish and chips");
    private static readonly Recipe SharedSecondRecipe = TestRecipes.Create("Pierogi");
    private static readonly string InvalidDateExceptionMessage = $"Invalid date specified. The date can not be before {Menu.MinDateInThePast} and must be in the near future.";

    [Theory]
    [ClassData(typeof(InvalidDatesTestDataProvider))]
    public void Create_ThrowsForInvalidDate(DateOnly invalidDate, DateOutOfRangeException.Cause underlyingCause)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1), AddMealAction.Create(SharedSecondRecipe, 2, 1)];
            
        // Act
        Action<DateOnly> createNewMenu = date => Menu.Create(date, mealsToAdd);
        
        // Assert
        createNewMenu.Invoking(x => x.Invoke(invalidDate))
            .Should().Throw<DateOutOfRangeException>()
            .Which.UnderlyingCause.Should().Be(underlyingCause);
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
        addMeal.Should().Throw<MealExistsAtPositionException>()
            .Which.Order.Should().Be(order);
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
        var exception = addMealToMenu.Invoking(x => x.Invoke(thirdMeal))
            .Should().Throw<MealAlreadyPresentInTheDayException>()
            .Which;
        using (new AssertionScope())
        {
            exception.Name.Should().Be(SharedFirstRecipe.Name);
            exception.Date.Should().Be(menu.Date);
        }
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
        addMeal.Should().Throw<InvalidMealOrderException>()
            .Which.UnderlyingCause.Should().Be(InvalidMealOrderException.Cause.ExceedsRange);
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