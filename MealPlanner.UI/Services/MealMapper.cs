using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.UI.Models;

namespace MealPlanner.UI.Services;

public class MealMapper
{
    public List<MealDto> MapMeals(IEnumerable<OrderedRecipeListItemResponse> orderedMeals)
    {
        var meals = orderedMeals.Select(x => new MealDto
        {
            Id = x.Id,
            Name = x.Name,
            Order = x.Order,
            Servings = x.Servings
        }).OrderBy(x => x.Order).ToList();
        return meals;
    }
    
    public List<PanelRecipe> MapPanelRecipes(GetRecipesResponse? response) =>
        response is null
            ? []
            : [.. response.Recipes
                .OrderBy(recipe => recipe.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(recipe => new PanelRecipe(recipe.Id, recipe.Name, recipe.Servings))];
}