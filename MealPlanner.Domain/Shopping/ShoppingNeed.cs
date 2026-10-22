using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.Shopping;

public class ShoppingNeed
{
    public int Id { get; private set; }
    public int IngredientId { get; private set; }
    public string IngredientName { get; private set; }
    public decimal Quantity { get; private set; }
    public MeasureUnit Unit { get; private set; }
    public int? ShelfLifeDays { get; private set; }
    public DateOnly ConsumptionDate { get; private set; }
}