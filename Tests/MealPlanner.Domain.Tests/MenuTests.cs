using AwesomeAssertions;

namespace MealPlanner.Domain.Tests;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Meal SharedFirstMeal = Meal.Create("Fish and chips");
    private static readonly Meal SharedSecondMeal = Meal.Create("Pierogi");
    private static readonly string InvalidDateExceptionMessage = $"Invalid date specified. The date can not be before {Menu.MinDateInThePast} and must be in the near future.";

    [Theory]
    [MemberData(nameof(InvalidDatesSource))]
    public void Create_ThrowsForInvalidDate(DateOnly invalidDate)
    {
        // Act
        Action<DateOnly> createNewMenu = date => Menu.Create(date);
        
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
        var result = Menu.Create(validDate);
        
        // Assert
        result.Date.Should().Be(validDate);
    }

    [Fact]
    public void AddMeal_WithoutOrder_SuccessfullyAddsMeal()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        
        // Act
        menu.AddMeal(SharedFirstMeal);
        
        // Assert
        menu.Items.Should().HaveCount(1);
        menu.GetMeal(0).Should().Be(SharedFirstMeal);
    }

    [Fact]
    public void AddMeal_WithoutOrder_KeepsOrder()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        menu.AddMeal(SharedFirstMeal);
        
        // Act
        menu.AddMeal(SharedSecondMeal);
        
        // Assert
        menu.Items.Should().HaveCount(2);
        menu.GetMeal(0).Should().Be(SharedFirstMeal);
        menu.GetMeal(1).Should().Be(SharedSecondMeal);
    }

    [Fact]
    public void AddMeal_WithMealAlreadyAdded_ThrowsException()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        menu.AddMeal(SharedFirstMeal);
        
        // Act
        Action<Meal> addMealToMenu = newMeal => menu.AddMeal(newMeal);
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(SharedFirstMeal))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Meal '{SharedFirstMeal}' is already present in the menu for {menu.Date}.");
    }

    [Fact]
    public void AddMeal_WithOrder_SuccessfullyAddsMeal()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        
        // Act
        menu.AddMeal(0, SharedFirstMeal);
        
        // Assert
        menu.Items.Should().HaveCount(1);
        menu.GetMeal(0)!.Name.Should().Be(SharedFirstMeal.Name);
    }

    [Fact]
    public void AddMeal_WithOrder_WhenOrderAlreadyTaken_ThrowsException()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        const int mealOrder = 0;
        menu.AddMeal(mealOrder, SharedFirstMeal);
        
        // Act
        Action<int, Meal> addMealToMenu = (order, meal) =>  menu.AddMeal(order, meal);
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(mealOrder, SharedSecondMeal))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"There is already a meal added as #{mealOrder + 1} in the day");
    }

    [Fact]
    public void AddMeal_WithOrder_WithMealAlreadyAdded_ThrowsException()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        menu.AddMeal(SharedFirstMeal);
        
        // Act
        Action<int, Meal> addMealToMenu = (order, meal) =>  menu.AddMeal(order, meal);
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(1, SharedFirstMeal))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Meal '{SharedFirstMeal}' is already present in the menu for {menu.Date}.");
    }

    [Fact]
    public void AddMeal_WithOrder_ThrowsExceptionForNegativeOrder()
    {
        // Arrange
        var menu = Menu.Create(SharedDate);
        
        // Act
        Action<int, Meal> addMealToMenu = (order, meal) => menu.AddMeal(order, meal);
        
        // Assert
        addMealToMenu.Invoking(x => x.Invoke(-1, SharedFirstMeal))
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