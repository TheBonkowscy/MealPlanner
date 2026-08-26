namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipePreparationsRequest(string Description, int LeadDays, bool Required);