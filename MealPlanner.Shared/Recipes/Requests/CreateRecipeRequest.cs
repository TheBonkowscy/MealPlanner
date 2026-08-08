namespace MealPlanner.Shared.Recipes.Requests;

public record CreateRecipeRequest(
    string Name,
    List<IngredientListItemRequest> Ingredients,
    List<StepListItemRequest> Steps);

public class StepListItemRequest(int Id, int Order, string Instructions);

public class IngredientListItemRequest(int Id, decimal Amount, string Unit);