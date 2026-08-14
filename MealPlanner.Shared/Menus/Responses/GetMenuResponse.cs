using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Shared.Menus.Responses;

public record GetMenuResponse(int Id, DateOnly Date, IEnumerable<OrderedRecipeListItemResponse> Meals);