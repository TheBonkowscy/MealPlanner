using System.Collections;
using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests;

public class RecipeTests
{
    private const string Name = "Fish and chips";
    private static readonly AddIngredientAction SharedIngredient = TestActions.AddFlour(0.75m, MeasureUnit.GlassCup);
    private static List<RecipeStep> SharedSteps => [RecipeStep.Create(1, "Step 1").Value, RecipeStep.Create(2, "Step 2").Value];
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithoutName_Fails(string invalidName)
    {   
        // Act
        var result = Recipe.Create(invalidName, 1, [SharedIngredient], SharedSteps);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Recipe.InvalidName(invalidName));
    }

    [Fact]
    public void Create_WithEmptyIngredients_Fails()
    {
        // Act
        var result = Recipe.Create(Name, 1, [], SharedSteps);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Recipe.InvalidRecipeIngredients);
    }

    [Fact]
    public void Create_WithEmptySteps_Fails()
    {
        // Act
        var result = Recipe.Create(Name, 1,  [SharedIngredient], []);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Recipe.InvalidSteps);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidServings_Fails(int invalidServings)
    {
        // Act
        var result = Recipe.Create(Name, invalidServings, [SharedIngredient], SharedSteps);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Recipe.InvalidServings(invalidServings));
    }

    [Fact]
    public void Create_Succeeds()
    {
        // Act
        var result = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Name.Should().Be(Name);
        result.Value.Ingredients.Should().HaveCount(1);
        result.Value.Steps.Should().HaveCount(2);
        result.Value.Servings.Should().Be(1);
    }

    [Fact]
    public void Create_NormalizesOrders_WhenStepsHaveGaps()
    {
        // Arrange
        List<RecipeStep> stepsWithGaps =
        [
            RecipeStep.Create(10, "Step 10").Value,
            RecipeStep.Create(20, "Step 20").Value,
            RecipeStep.Create(30, "Step 30").Value
        ];

        // Act
        var result = Recipe.Create(Name, 1, [SharedIngredient], stepsWithGaps);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2, 3], options => options.WithStrictOrdering());
    }

    [Fact]
    public void UpdateStep_Fails_WhenStepWasNotFound()
    {
        // Arrange
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps).Value;
        
        // Act
        var result = recipe.UpdateStep(-1, 1, "Updated instructions");
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipeStep.NotFound);
    }

    [Theory]
    [ClassData(typeof(RecipeStepOrderingTests))]
    public void UpdateStep_ReordersStepsOnInsert(Recipe recipe, int stepId, int updatedOrder, string updatedInstructions)
    {
        // Act
        var result = recipe.UpdateStep(stepId, updatedOrder, updatedInstructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
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
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps).Value;

        // Act
        var result = recipe.AddStep(2, "New Step 2");

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        recipe.Steps.Should().HaveCount(3);
        recipe.Steps.Select(s => s.Order).Should().BeEquivalentTo([1, 2, 3], options => options.WithStrictOrdering());
        recipe.Steps[1].Instructions.Should().Be("New Step 2");
    }

    [Fact]
    public void AddStep_WithOrderExceedingCount_AppendsToTheEnd()
    {
        // Arrange
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], SharedSteps).Value;

        // Act
        var result = recipe.AddStep(99, "Far step");

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
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
            RecipeStep.Create(1, "Step 1").Value,
            RecipeStep.Create(2, "Step 2").Value,
            RecipeStep.Create(3, "Step 3").Value
        };
        RandomId.Set([.. steps]);
        var recipe = Recipe.Create(Name, 1, [SharedIngredient], steps).Value;
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
                .Select(order => RecipeStep.Create(order, $"Instructions for step #{order}").Value).ToList();
            RandomId.Set([.. steps]);
            return Recipe.Create($"Recipe_{Guid.NewGuid()}", 1, [SharedIngredient], steps).Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}