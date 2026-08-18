namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipeStepRequest(int Order, string Instructions);