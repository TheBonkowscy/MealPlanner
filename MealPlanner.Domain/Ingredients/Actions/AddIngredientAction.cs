using MealPlanner.Domain.Shared;

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
    
    public static Result<AddIngredientAction> Create(Ingredient ingredient, decimal quantity, MeasureUnit unit)
    {
        var errors = new List<Error>();
        errors.AddRule(ingredient.IsApplicableUnit(unit), DomainErrors.Ingredient.UnitNotApplicable(unit, ingredient.Name))
            .AddRule(quantity > 0, DomainErrors.Ingredient.InvalidQuantity(quantity, ingredient.Name));

        if (errors.Count != 0)
        {
            return Result.Failure<AddIngredientAction>(errors);
        }
        
        return Result.Success(new AddIngredientAction
        {
            Ingredient = ingredient,
            Quantity = quantity,
            Unit = unit
        });
    }
}