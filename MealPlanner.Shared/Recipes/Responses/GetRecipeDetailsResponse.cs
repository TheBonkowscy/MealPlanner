using MealPlanner.Shared.Shared;

namespace MealPlanner.Shared.Recipes.Responses;

public record GetRecipeDetailsResponse(
    int Id,
    string Name,
    IEnumerable<UsedIngredientDetailsResponse> Ingredients,
    IEnumerable<StepDetailsResponse> Steps);
    
    public record UsedIngredientDetailsResponse(int Id, string Name, decimal Quantity, MeasureUnitDto MeasureUnit);

    public record StepDetailsResponse(int Id, int Order, string Description);