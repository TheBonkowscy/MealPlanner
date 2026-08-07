using AwesomeAssertions;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class MealTests
{
    private const string Name = "Fish and chips";
    
    [Fact]
    public void Create_WithoutName_ThrowsException()
    {   
        // Act
        Action<string> createMeal = (name) => Meal.Create(name);
        
        // Assert
        createMeal.Invoking(x => x.Invoke(""))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Please specify a name of the meal");
    }

    [Fact]
    public void Create_WithName_Succeeds()
    {
        // Act
        var meal = Meal.Create(Name);
        
        // Assert
        meal.Should().NotBeNull();
        meal.Name.Should().Be(Name);
    }

    [Fact]
    public void Create_WithEmptyIngredients_ThrowsException()
    {
        // Act
        Action<string, List<AddIngredientAction>> createMeal = (name, ingredients) => Meal.Create(name, ingredients);
        
        // Assert
        createMeal.Invoking(x => x.Invoke(Name, []))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("At least one ingredient must be specified");
    }

    [Fact]
    public void Create_WithIngredients_Succeeds()
    {
        // Arrange
        var addedIngredients = TestActions.AddFlour(0.75m, TestIngredientsUnits.Cups());
        
        // Act
        Action<string, List<AddIngredientAction>> createMeal = (name, ingredients) => Meal.Create(name, ingredients);
        
        // Assert
        createMeal.Invoking(x => x.Invoke("", [addedIngredients]))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Please specify a name of the meal");
    }

    [Fact]
    public void Create_WithNameAndIngredients_Succeeds()
    {
        // Arrange
        var addedIngredients = TestActions.AddFlour(0.75m, TestIngredientsUnits.Cups());
        
        // Act
        Action<string, List<AddIngredientAction>> createMeal = (name, ingredients) => Meal.Create(name, ingredients);
        
        // Assert
        createMeal.Invoking(x => x.Invoke("", [addedIngredients]))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Please specify a name of the meal");        
    }
}