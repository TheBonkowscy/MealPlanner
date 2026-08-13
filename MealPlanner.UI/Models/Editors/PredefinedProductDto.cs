using MealPlanner.Shared.Shared;

namespace MealPlanner.UI.Models.Editors;

public record PredefinedProductDto(int Id, string Name, MeasureUnitDto[] Units);