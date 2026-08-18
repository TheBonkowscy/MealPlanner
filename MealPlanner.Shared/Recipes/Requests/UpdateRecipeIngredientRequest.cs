namespace MealPlanner.Shared.Recipes.Requests;

public record UpdateRecipeIngredientRequest(int Id, decimal Quantity, string Unit);