using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Shared.Shared;
using Microsoft.Extensions.Localization;

namespace MealPlanner.Services.Recipes;

public class MeasureUnitMapper(IStringLocalizer<Translations> localizer)
{
    public MeasureUnitDto Map(MeasureUnit measureUnit)
    {
        var unitAsString = measureUnit.ToString();
        var displayName = localizer[unitAsString];
        return new MeasureUnitDto(displayName, unitAsString);
    }
    
    public MeasureUnit Map(string source) => Enum.Parse<MeasureUnit>(source);
}