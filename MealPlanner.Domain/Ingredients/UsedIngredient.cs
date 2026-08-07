using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Ingredients;

public class UsedIngredient
{
    public int MealId { get; private set; }
    
    public Meal Meal { get; private set; }

    public int IngredientId { get; private set; }
    
    public Ingredient Ingredient { get; private set; }

    public decimal Quantity { get; private set; }
    
    public int UnitId { get; private set; }
    
    public IngredientUnit Unit { get; private set; }

    private UsedIngredient()
    {
        // For EF Core
    }
    
    private UsedIngredient(Meal meal, Ingredient ingredient, decimal quantity, IngredientUnit unit)
    {
        Meal = meal;
        MealId = meal.Id;
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Quantity = quantity;
        Unit = unit;
    }

    public static UsedIngredient Create(Meal meal, AddIngredientAction action) =>
        new(meal, action.Ingredient, action.Quantity, action.Unit);
}