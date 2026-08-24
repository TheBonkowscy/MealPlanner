using AwesomeAssertions;
using AwesomeAssertions.Execution;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Recipe SharedFirstRecipe = TestRecipes.Create("Fish and chips");
    private static readonly Recipe SharedSecondRecipe = TestRecipes.Create("Pierogi");

    [Theory]
    [ClassData(typeof(InvalidDatesTestDataProvider))]
    public void Create_Fails_WhenDateIsInvalid(DateOnly invalidDate, Error underlyingCause)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
            
        // Act
        var result = Menu.Create(invalidDate, mealsToAdd);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(underlyingCause);
    }

    [Theory]
    [MemberData(nameof(ValidDatesSource))]
    public void Create_CreatesSuccessfully(DateOnly validDate)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
        
        // Act
        var result = Menu.Create(validDate, mealsToAdd);
        
        // Assert
        result.Value.Date.Should().Be(validDate);
    }

    [Fact]
    public void AddMeal_SuccessfullyAddsMeal_KeepsOrder()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 2, 1).Value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Meals.Should().HaveCount(2);
        menu.GetRecipe(1).Should().Be(SharedFirstRecipe);
        menu.GetRecipe(2).Should().Be(SharedSecondRecipe);
    }

    [Fact]
    public void AddMeal_Fails_WhenOrderAlreadyTaken()
    {
        // Arrange
        const int order = 1;
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, order, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, order, 1).Value);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyExistsAtPosition(order));
    }

    [Fact]
    public void AddMeal_WithMealAlreadyAdded_FailsException()
    {
        // Arrange
        var firstMeal = AddMealAction.Create(SharedFirstRecipe, 1, 1).Value;
        List<AddMealAction> mealsToAdd = [firstMeal, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        var thirdMeal = AddMealAction.Create(SharedFirstRecipe, 3, 1).Value;
        
        // Act
        var result = menu.AddMeal(thirdMeal);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay(thirdMeal.Recipe.Name));
    }

    [Fact]
    public void AddMeal_Fails_WhenOrderIsOutOfBounds()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 999, 1).Value);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
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