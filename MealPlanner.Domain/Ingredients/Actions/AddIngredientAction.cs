using MealPlanner.Domain.Ingredients.Exceptions;

namespace MealPlanner.Domain.Ingredients.Actions;

// TODO: consider removing this in the future
public class AddIngredientAction
{
    public Ingredient Ingredient { get; private set; }
    public decimal Quantity { get; private set; }
    
    public MeasureUnit Unit { get; private set; }

    private AddIngredientAction()
    {
        // Prevent creating without validation
    }
    
    public static AddIngredientAction Create(Ingredient ingredient, decimal quantity, MeasureUnit unit)
    {
        if (!ingredient.IsApplicableUnit(unit))
        {
            throw new InvalidOperationException("Ingredient does not support the specified unit");
        }

        InvalidIngredientQuantityException.ThrowIfQuantityIsInvalid(quantity);
        
        return new AddIngredientAction
        {
            Ingredient = ingredient,
            Quantity = quantity,
            Unit = unit
        };
    }
}