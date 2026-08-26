namespace MealPlanner.Shared.Recipes.Requests;

public record UpdateRecipePreparationsInfoRequest(string Description, int LeadDays, bool Required);