namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipeRequest(
    string Name,
    int Servings,
    List<AddIngredientRequest> Ingredients,
    List<AddRecipeStepRequest> Steps,
    List<AddRecipePreparationsRequest> Preparations);

public record AddIngredientRequest(int Id, decimal Quantity, string Unit);

public record AddRecipeStepRequest(int Order, string Instructions);

public record AddRecipePreparationsRequest(string Description, int LeadDays, bool Required);