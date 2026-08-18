namespace MealPlanner.Shared.Recipes.Requests;

public record UpdateRecipeStepRequest(int Id, int Order, string Instructions);