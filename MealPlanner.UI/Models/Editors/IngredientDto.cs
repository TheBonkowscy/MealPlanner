using MealPlanner.UI.Components.Pages.Editors.Recipes;

namespace MealPlanner.UI.Models.Editors;

public class IngredientDto
{
    
    public int Id { get; set; }
    public string Name { get; set; }
    public UnitDto[] Units { get; set; }
    public decimal Quantity { get; set; } = 1;
    public PredefinedProductDto? SelectedProduct { get; set; }

    public UnitDto? SelectedUnit { get; set; } = null;
    
    public IReadOnlyList<UnitDto> AvailableUnits => SelectedProduct?.Units ?? [];
    
    public string ZoneIdentifier { get; set; } = "IngredientsZone";
}