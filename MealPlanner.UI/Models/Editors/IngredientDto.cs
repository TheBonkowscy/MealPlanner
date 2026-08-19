using MealPlanner.Shared.Shared;

namespace MealPlanner.UI.Models.Editors;

public class IngredientDto : IReorderable
{
    public string ZoneIdentifier => "IngredientsZone";

    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; } = 1;
    public PredefinedProductDto? SelectedProduct { get; set; }

    public MeasureUnitDto? SelectedUnit { get; set; } = null;
    
    public IReadOnlyList<MeasureUnitDto> AvailableUnits => SelectedProduct?.Units ?? [];

    public bool IsPersisted { get; set; }
    
}