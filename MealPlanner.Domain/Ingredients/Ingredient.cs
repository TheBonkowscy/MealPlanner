using MealPlanner.Domain.Ingredients.Exceptions;

namespace MealPlanner.Domain.Ingredients;

public class Ingredient
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }

    public List<MeasureUnit> ApplicableUnits { get; private set; } = [];

    private Ingredient()
    {
        // For EF Core
    }
    
    private Ingredient(string name, List<MeasureUnit> applicableUnits)
    {
        Name = name;
        ApplicableUnits = applicableUnits;
    }

    public static Ingredient Create(string name, List<MeasureUnit> applicableUnits)
    {
        MissingIngredientNameException.ThrowIfNameIsInvalid(name);
        ValidateUnitsAndThrow(applicableUnits);

        return new Ingredient(name, applicableUnits);
    }

    private static void ValidateUnitsAndThrow(List<MeasureUnit> applicableUnits) => MissingMeasureUnitsException.ThrowIfEmpty(applicableUnits);

    public bool IsApplicableUnit(MeasureUnit unit) => ApplicableUnits.Any(x => x == unit);

    public void UpdateApplicableUnits(List<MeasureUnit> applicableUnits)
    {
        ValidateUnitsAndThrow(applicableUnits);
        ApplicableUnits = applicableUnits;
    }
}