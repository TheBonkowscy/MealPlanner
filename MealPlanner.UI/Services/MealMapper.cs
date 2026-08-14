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
            Order = x.Order
        }).OrderBy(x => x.Order).ToList();
        return meals;
    }
}