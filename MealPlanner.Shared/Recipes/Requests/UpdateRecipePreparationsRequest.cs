namespace MealPlanner.Shared.Recipes.Requests;

public record UpdateRecipePreparationsRequest(int Id, string Description, int LeadDays, bool Required);