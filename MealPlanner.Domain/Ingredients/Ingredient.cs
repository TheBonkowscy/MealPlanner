namespace MealPlanner.Domain.Ingredients;

public class Ingredient
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }

    private List<MeasureUnit> _applicableUnits = [];
    
    public IReadOnlyList<MeasureUnit> ApplicableUnits 
    { 
        get => _applicableUnits;
        private set => _applicableUnits = [..value]; 
    }

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
        ValidateNameAndThrow(name);
        ValidateUnitsAndThrow(applicableUnits);

        return new Ingredient(name, applicableUnits);
    }

    private static void ValidateNameAndThrow(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(null, "Ingredient name cannot be null or whitespace");
        }
    }

    private static void ValidateUnitsAndThrow(List<MeasureUnit> applicableUnits)
    {
        if (applicableUnits.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one applicable unit", null, null);
        }
    }

    public bool IsApplicableUnit(MeasureUnit unit) => ApplicableUnits.Any(x => x.Id == unit.Id);
}