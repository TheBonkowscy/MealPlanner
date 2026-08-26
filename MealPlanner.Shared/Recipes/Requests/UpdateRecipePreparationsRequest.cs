namespace MealPlanner.Shared.Recipes.Requests;

public record UpdateRecipePreparationsRequest(string Description, int LeadDays, bool Required);