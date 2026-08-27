namespace MealPlanner.Shared.Recipes.Responses;

public record GetRecipePreparationsDetailsResponse(IEnumerable<RecipePreparationsDescriptionResponse> Preparations);

public record RecipePreparationsDescriptionResponse(
    int RecipeId,
    string RecipeName,
    DateOnly MealDate,
    int LeadDays,
    bool Required,
    string Description);