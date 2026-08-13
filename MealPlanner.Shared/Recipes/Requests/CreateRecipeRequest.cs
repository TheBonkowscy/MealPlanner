namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipeRequest(
    string Name,
    List<AddIngredientRequest> Ingredients,
    List<AddStepRequest> Steps);

public record AddIngredientRequest(int Id, decimal Quantity, string Unit);

public record AddStepRequest(int Order, string Instructions);