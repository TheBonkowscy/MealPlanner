using AwesomeAssertions;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class MealTests
{
    private const string Name = "Fish and chips";
    private static readonly AddIngredientAction SharedIngredient = TestActions.AddFlour(0.75m, TestIngredientsUnits.Cups());
    private static readonly List<RecipeStep> SharedSteps = [RecipeStep.Create(1, "Step 1"), RecipeStep.Create(1, "Step 2")];
    
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
        Action<string, List<AddIngredientAction>, List<RecipeStep>> createMeal = (name, ingredients, recipeSteps) => Meal.Create(name, ingredients, recipeSteps);
        
        // Assert
        createMeal.Invoking(x => x.Invoke(Name, [], SharedSteps))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("At least one ingredient must be specified");
    }

    [Fact]
    public void Create_WithEmptySteps_ThrowsException()
    {
        // Act
        Action<string, List<AddIngredientAction>, List<RecipeStep>> createMeal = (name, ingredients, recipeSteps) => Meal.Create(name, ingredients, recipeSteps);
        
        // Assert
        createMeal.Invoking(x => x.Invoke(Name, [SharedIngredient], []))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("At least one recipe step must be specified");
    }

    [Fact]
    public void Create_WithNameAndIngredients_Succeeds()
    {
        // Act
        Action<string, List<AddIngredientAction>, List<RecipeStep>> createMeal = (name, ingredients, recipeSteps) => Meal.Create(name, ingredients, recipeSteps);
        
        // Assert
        createMeal.Invoking(x => x.Invoke("", [SharedIngredient], SharedSteps))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Please specify a name of the meal");        
    }
}