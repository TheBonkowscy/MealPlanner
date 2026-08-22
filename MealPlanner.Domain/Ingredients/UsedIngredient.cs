using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Domain.Ingredients;

public class UsedIngredient
{
    public int RecipeId { get; private set; }
    
    public Recipe Recipe { get; private set; }

    public int IngredientId { get; private set; }
    
    public Ingredient Ingredient { get; private set; }

    public decimal Quantity { get; private set; }
    
    public MeasureUnit Unit { get; private set; }

    private UsedIngredient()
    {
        // For EF Core
    }
    
    private UsedIngredient(Recipe recipe, Ingredient ingredient, decimal quantity, MeasureUnit unit)
    {
        Recipe = recipe;
        RecipeId = recipe.Id;
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Quantity = quantity;
        Unit = unit;
    }

    public static Result<UsedIngredient> Create(Recipe? recipe, AddIngredientAction action)
    {
        var errors = new List<Error>();
        errors.AddRule(recipe is null, DomainErrors.Recipe.IsNull);
        errors.AddRule(action.Quantity <= 0, DomainErrors.Ingredients.InvalidQuantity);
        return errors.Count > 0 ? Result.Failure<UsedIngredient>(errors) : Result.Success(new UsedIngredient(recipe, action.Ingredient, action.Quantity, action.Unit));
    }

    public Result UpdateQuantity(decimal quantity)
    {
        var errors = new List<Error>();
        errors.AddRule(quantity <= 0, DomainErrors.Ingredients.InvalidQuantity);
        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }
        
        Quantity = quantity;
        return Result.Success();
    }
}