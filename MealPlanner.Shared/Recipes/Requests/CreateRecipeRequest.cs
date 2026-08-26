namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipeRequest(
    string Name,
    int Servings,
    List<AddIngredientRequest> Ingredients,
    List<AddRecipeStepRequest> Steps);

public record AddIngredientRequest(int Id, decimal Quantity, string Unit);

public record AddRecipeStepRequest(int Order, string Instructions);