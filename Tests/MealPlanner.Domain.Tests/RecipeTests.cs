using System.Collections;
using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Ingredients.Exceptions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Recipes.Exceptions;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests;

public class RecipeTests
{
    private const string Name = "Fish and chips";
    private static readonly AddIngredientAction SharedIngredient = TestActions.AddFlour(0.75m, MeasureUnit.GlassCup);
    private static List<RecipeStep> SharedSteps => [RecipeStep.Create(1, "Step 1"), RecipeStep.Create(2, "Step 2")];
    
    [Fact]
    public void Create_WithoutName_ThrowsException()
    {   
        // Act
        Action<string> createRecipe = (name) => Recipe.Create(name, 1, [SharedIngredient], SharedSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(""))
            .Should().Throw<MissingRecipeNameException>();
    }

    [Fact]
    public void Create_WithEmptyIngredients_ThrowsException()
    {
        // Act
        Action<string, int, List<AddIngredientAction>, List<RecipeStep>> createRecipe = (name, servings, ingredients, recipeSteps) => Recipe.Create(name, servings, ingredients, recipeSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(Name, 1, [], SharedSteps))
            .Should().Throw<MissingIngredientsException>();
    }

    [Fact]
    public void Create_WithEmptySteps_ThrowsException()
    {
        // Act
        Action<string, int, List<AddIngredientAction>, List<RecipeStep>> createRecipe = (name, servings, ingredients, recipeSteps) => Recipe.Create(name, servings, ingredients, recipeSteps);
        
        // Assert
        createRecipe.Invoking(x => x.Invoke(Name, 1, [SharedIngredient], []))
            .Should().Throw<MissingRecipeStepsException>();
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
            .Should().Throw<InvalidNumberOfServingsException>();
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

    [Fact]
    public void Create_NormalizesOrders_WhenStepsHaveGaps()
    {
        // Arrange
        List<RecipeStep> stepsWithGaps =
        [
            RecipeStep.Create(10, "Step 10"),
            RecipeStep.Create(20, "Step 20"),
            RecipeStep.Create(30, "Step 30")
        ];

        // Act
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], stepsWithGaps);

        // Assert
        recipe.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2, 3], options => options.WithStrictOrdering());
    }

    [Fact]
    public void UpdateStep_Throws_WhenStepWasNotFound()
    {
        // Arrange
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps);
        
        // Act
        var result = () => recipe.UpdateStep(-1, 1, "Updated instructions");
        
        // Assert
        result.Invoking(x => x.Invoke())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Recipe step could not be found");
    }

    [Theory]
    [ClassData(typeof(RecipeStepOrderingTests))]
    public void UpdateStep_ReordersStepsOnInsert(Recipe recipe, int stepId, int updatedOrder, string updatedInstructions)
    {
        // Act
        recipe.UpdateStep(stepId, updatedOrder, updatedInstructions);
        
        // Assert
        var updatedStep = recipe.Steps.First(x => x.Id == stepId);
        updatedStep.Order.Should().Be(updatedOrder);
        updatedStep.Instructions.Should().Be(updatedInstructions);

        var actualOrders = recipe.Steps.Select(s => s.Order).ToList();
        var expectedOrders = Enumerable.Range(1, recipe.Steps.Count).ToList();
        actualOrders.Should().BeEquivalentTo(expectedOrders, options => options.WithStrictOrdering());
    }

    [Fact]
    public void AddStep_InsertsStepAndReindexesRest()
    {
        // Arrange
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps);

        // Act
        recipe.AddStep(2, "New Step 2");

        // Assert
        recipe.Steps.Should().HaveCount(3);
        recipe.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2, 3], options => options.WithStrictOrdering());
        recipe.Steps[1].Instructions.Should().Be("New Step 2");
    }

    [Fact]
    public void AddStep_WithOrderExceedingCount_AppendsToTheEnd()
    {
        // Arrange
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps);

        // Act
        recipe.AddStep(99, "Far step");

        // Assert
        recipe.Steps.Should().HaveCount(3);
        recipe.Steps.Last().Order.Should().Be(3);
        recipe.Steps.Last().Instructions.Should().Be("Far step");
    }

    [Fact]
    public void RemoveStep_RemovesGapAndReindexesRemainingSteps()
    {
        // Arrange
        var steps = new List<RecipeStep>
        {
            RecipeStep.Create(1, "Step 1"),
            RecipeStep.Create(2, "Step 2"),
            RecipeStep.Create(3, "Step 3")
        };
        RandomId.Set([.. steps]);
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], steps);
        var stepToRemove = recipe.Steps[1];

        // Act
        recipe.RemoveStep(stepToRemove);

        // Assert
        recipe.Steps.Should().HaveCount(2);
        recipe.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2], options => options.WithStrictOrdering());
        recipe.Steps[0].Instructions.Should().Be("Step 1");
        recipe.Steps[1].Instructions.Should().Be("Step 3");
    }

    private class RecipeStepOrderingTests : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var firstRecipe = CreateTestData(3);
            var firstStepId = firstRecipe.Steps.FirstOrDefault(x => x.Order == 1)?.Id ?? throw new InvalidOperationException();
            yield return [firstRecipe, firstStepId, 1, $"Updated instructions_{Guid.NewGuid()}"];
            
            var secondRecipe = CreateTestData(3);
            var secondStepId = secondRecipe.Steps.FirstOrDefault(x => x.Order == 2)?.Id ?? throw new InvalidOperationException();
            yield return [secondRecipe, secondStepId, 2, $"Updated instructions_{Guid.NewGuid()}"];
            
            var thirdRecipe = CreateTestData(3);
            var thirdStepId = thirdRecipe.Steps.FirstOrDefault(x => x.Order == 3)?.Id ?? throw new InvalidOperationException();
            yield return [thirdRecipe, thirdStepId, 3, $"Updated instructions_{Guid.NewGuid()}"];
        }

        private static Recipe CreateTestData(int numberOfSteps)
        {
            var steps = Enumerable.Range(1, numberOfSteps)
                .Select(order => RecipeStep.Create(order, $"Instructions for step #{order}")).ToList();
            RandomId.Set([.. steps]);
            return Recipe.Create($"Recipe_{Guid.NewGuid()}", 1, [SharedIngredient], steps);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}