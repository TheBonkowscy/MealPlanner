namespace MealPlanner.Domain.Ingredients.Actions;

// TODO: consider removing this in the future
public class AddIngredientAction
{
    public Ingredient Ingredient { get; private set; }
    public decimal Quantity { get; private set; }
    
    public IngredientUnit Unit { get; private set; }

    private AddIngredientAction()
    {
        // Prevent creating via validation
    }
    
    public static AddIngredientAction Create(Ingredient ingredient, decimal quantity, IngredientUnit unit)
    {
        if (!ingredient.IsApplicableUnit(unit))
        {
            throw new InvalidOperationException("Ingredient does not support the specified unit");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(null, "Ingredient quantity must be positive");
        }
        
        return new AddIngredientAction
        {
            Ingredient = ingredient,
            Quantity = quantity,
            Unit = unit
        };
    }
}