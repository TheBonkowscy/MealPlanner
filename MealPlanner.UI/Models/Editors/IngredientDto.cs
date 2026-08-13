using MealPlanner.Shared.Shared;
using MealPlanner.UI.Components.Pages.Editors.Recipes;

namespace MealPlanner.UI.Models.Editors;

public class IngredientDto : IReorderable
{
    public string ZoneIdentifier => "IngredientsZone";

    public int Id { get; set; }
    public string Name { get; set; }
    public MeasureUnitDto[] Units { get; set; }
    public decimal Quantity { get; set; } = 1;
    public PredefinedProductDto? SelectedProduct { get; set; }

    public MeasureUnitDto? SelectedUnit { get; set; } = null;
    
    public IReadOnlyList<MeasureUnitDto> AvailableUnits => SelectedProduct?.Units ?? [];
    
}