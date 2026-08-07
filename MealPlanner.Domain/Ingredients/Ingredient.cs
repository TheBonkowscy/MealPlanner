namespace MealPlanner.Domain.Ingredients;

public class Ingredient
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }

    private List<IngredientUnit> _applicableUnits = [];
    
    public IReadOnlyList<IngredientUnit> ApplicableUnits 
    { 
        get => _applicableUnits;
        private set => _applicableUnits = [..value]; 
    }

    private Ingredient()
    {
        // For EF Core
    }
    
    private Ingredient(string name, List<IngredientUnit> applicableUnits)
    {
        Name = name;
        ApplicableUnits = applicableUnits;
    }

    public static Ingredient Create(string name, List<IngredientUnit> applicableUnits)
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

    private static void ValidateUnitsAndThrow(List<IngredientUnit> applicableUnits)
    {
        if (applicableUnits.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one applicable unit", null, null);
        }
    }

    public bool IsApplicableUnit(IngredientUnit unit) => ApplicableUnits.Any(x => x.Id == unit.Id);
}