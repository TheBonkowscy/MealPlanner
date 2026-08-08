using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Ingredients;

public class UsedIngredient
{
    public int RecipeId { get; private set; }
    
    public Recipe Recipe { get; private set; }

    public int IngredientId { get; private set; }
    
    public Ingredient Ingredient { get; private set; }

    public decimal Quantity { get; private set; }
    
    public int UnitId { get; private set; }
    
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

    public static UsedIngredient Create(Recipe recipe, AddIngredientAction action) =>
        new(recipe, action.Ingredient, action.Quantity, action.Unit);
}