using MealPlanner.Shared.Shared;

namespace MealPlanner.Shared.Recipes.Responses;

public record GetRecipeDetailsResponse(
    int Id,
    string Name,
    int Servings,
    IEnumerable<UsedIngredientDetailsResponse> Ingredients,
    IEnumerable<StepDetailsResponse> Steps,
    IEnumerable<PreparationsDetailsResponse> Preparations);
    
    public record UsedIngredientDetailsResponse(int Id, string Name, decimal Quantity, MeasureUnitDto MeasureUnit);

    public record StepDetailsResponse(int Id, int Order, string Instructions);

    public record PreparationsDetailsResponse(int Id, string Description, int LeadDays, bool Required);