using MealPlanner.UI.Models;

namespace MealPlanner.UI.Services;

public class MealMapper
{
    public List<MealDto> MapMeals(Dictionary<int, string> orderedMeals)
    {
        var meals = orderedMeals.Select(x => new MealDto
        {
            Name = x.Value,
            Order = x.Key,
            ZoneIdentifier = "DayMenuZone"
        }).OrderBy(x => x.Order).ToList();
        return meals;
    }
}