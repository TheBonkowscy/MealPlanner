using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class RecipeTests
{
    private const string Name = "Fish and chips";
    private static readonly AddIngredientAction SharedIngredient = TestActions.AddFlour(0.75m, MeasureUnit.GlassCup);
    private static readonly List<RecipeStep> SharedSteps = [RecipeStep.Create(1, "Step 1"), RecipeStep.Create(1, "Step 2")];
    
    [Fact]
    public void Create_WithoutName_ThrowsException()
    {   
        // Act
        Action<string> createRecipe = (name) => Recipe.Create(name, 1, [SharedIngredient], SharedSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(""))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Please specify a name of the recipe");
    }

    [Fact]
    public void Create_WithEmptyIngredients_ThrowsException()
    {
        // Act
        Action<string, int, List<AddIngredientAction>, List<RecipeStep>> createRecipe = (name, servings, ingredients, recipeSteps) => Recipe.Create(name, servings, ingredients, recipeSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(Name, 1, [], SharedSteps))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("At least one ingredient must be specified");
    }

    [Fact]
    public void Create_WithEmptySteps_ThrowsException()
    {
        // Act
        Action<string, int, List<AddIngredientAction>, List<RecipeStep>> createRecipe = (name, servings, ingredients, recipeSteps) => Recipe.Create(name, servings, ingredients, recipeSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(Name, 1, [SharedIngredient], []))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("At least one recipe step must be specified");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidServings_ThrowsException(int invalidServings)
    {
        // Act
        Action<string, int, List<AddIngredientAction>, List<RecipeStep>> createRecipe = (name, servings, ingredients, recipeSteps) => Recipe.Create(name, servings, ingredients, recipeSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(Name, invalidServings, [SharedIngredient], SharedSteps))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Recipe must yield at least one serving");
    }

    [Fact]
    public void Create_Succeeds()
    {
        // Act
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps);
        
        // Assert
        recipe.Should().NotBeNull();
        recipe.Name.Should().Be(Name);
    }
}