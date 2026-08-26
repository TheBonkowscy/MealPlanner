namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipePreparationsInfoRequest(string Description, int LeadDays, bool Required);