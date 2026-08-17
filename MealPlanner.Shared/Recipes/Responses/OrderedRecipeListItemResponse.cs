namespace MealPlanner.Shared.Recipes.Responses;

public record OrderedRecipeListItemResponse(int Id, int Order, int Servings, string Name);