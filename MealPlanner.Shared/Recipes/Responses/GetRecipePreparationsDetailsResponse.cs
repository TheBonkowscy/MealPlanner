namespace MealPlanner.Shared.Recipes.Responses;

public record GetRecipePreparationsDetailsResponse(IEnumerable<RecipePreparationsDescriptionResponse> Preparations)
{
    public static GetRecipePreparationsDetailsResponse Empty => new([]);
}

public record RecipePreparationsDescriptionResponse(
    int RecipeId,
    string RecipeName,
    DateOnly MealDate,
    int LeadDays,
    bool Required,
    string Description);