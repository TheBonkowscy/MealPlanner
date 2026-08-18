using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeUpdaterTests
{
    private readonly Mock<IStringLocalizer<Translations>> _localizer = new();
    private readonly RecipeUpdater _sut;

    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("PreExistingRecipe");
    private readonly List<RecipeStep> _recipeSteps = [];
    private readonly List<Recipe> _recipes = [];
    
    public RecipeUpdaterTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        ctx.Setup(x => x.Recipes.Add(It.IsAny<Recipe>())).Callback<Recipe>(recipe =>
        {
            RandomId.Set(recipe);
            _recipes.Add(recipe);

            var steps = recipe.Steps.ToArray();
            RandomId.Set(steps);
            _recipeSteps.AddRange(steps);
        });
        
        _recipes.Add(PreExistingRecipe);
        
        ctx.Setup(x => x.RecipeSteps).ReturnsDbSet(_recipeSteps);
        ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _sut = new RecipeUpdater(ctx.Object, new RecipeMapper(new MeasureUnitMapper(_localizer.Object)));
    }
    
    [Fact]
    public async Task Update_Throws_WhenRecipeDoesNotExist()
    {
        // Arrange
        var request = new UpdateRecipeRequest(Guid.NewGuid().ToString(), 1);
        
        // Act
        var result = () => _sut.Update(999, request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Recipe could not be found");
    }
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public async Task Update_Throws_WhenNameIsInvalid(string name)
    {
        // Arrange
        var request = new UpdateRecipeRequest(name, 1);
        
        // Act
        var result = () => _sut.Update(PreExistingRecipe.Id, request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("Please specify a name of the recipe");
    }
    
    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public async Task Update_Throws_WhenServingsAreOutOfRange(int servings)
    {
        // Arrange
        var request = new UpdateRecipeRequest(Guid.NewGuid().ToString(), servings);
        
        // Act
        var result = () => _sut.Update(PreExistingRecipe.Id, request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Recipe must yield at least one serving");
    }
    
    [Fact]
    public async Task Update_Succeeds()
    {
        // Arrange
        var request = new UpdateRecipeRequest("New recipe name", 15);
        
        // Act
        var result = await _sut.Update(PreExistingRecipe.Id, request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Servings.Should().Be(request.Servings);
    }
}