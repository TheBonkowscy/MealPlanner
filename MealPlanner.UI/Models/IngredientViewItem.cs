namespace MealPlanner.UI.Models;

public class IngredientViewItem
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}