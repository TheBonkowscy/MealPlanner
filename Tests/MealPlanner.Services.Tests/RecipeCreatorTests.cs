using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests;

public class RecipeCreatorTests
{
    private readonly Mock<IStringLocalizer<MeasureUnitMapper>> _localizer = new();
    private readonly RecipeCreator _sut;

    private static readonly Ingredient PreExistingIngredient = Ingredient.Create("PreExistingIngredient", [MeasureUnit.Bottle]);
    private readonly List<Ingredient> _ingredients = [PreExistingIngredient];
    private readonly List<RecipeStep> _recipeSteps = [];
    private readonly List<Recipe> _recipes = [];
    
    public RecipeCreatorTests()
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
        
        ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        ctx.Setup(x => x.RecipeSteps).ReturnsDbSet(_recipeSteps);
        
        ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _sut = new RecipeCreator(ctx.Object, new MeasureUnitMapper(_localizer.Object));
    }
    
    [Fact]
    public async Task Create_Throws_WhenRecipeAlreadyExists()
    {
        // Arrange
        var request = NewRequest();
        await _sut.Create(request, CancellationToken.None);
        
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Recipe '{request.Name}' already exists");
    }
    
    [Fact]
    public async Task Create_Throws_WhenIngredientDoesNotExist()
    {
        // Arrange
        var request = NewRequest();
        request.Ingredients.Add(new AddIngredientRequest(Random.Shared.Next(100, 1000), 1, nameof(MeasureUnit.Bottle)));
        
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more ingredients not found");
    }
    // 3. success
    
    [Fact]
    public async Task Create_Succeeds()
    {
        // Arrange
        var request = NewRequest();
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    private static CreateRecipeRequest NewRequest()
    {
        var ingredient = new AddIngredientRequest(PreExistingIngredient.Id, 1, nameof(MeasureUnit.Bottle));
        var step = new AddStepRequest(1, "Step 1");
        return new CreateRecipeRequest(Guid.NewGuid().ToString(), [ingredient], [step]);
    }
}