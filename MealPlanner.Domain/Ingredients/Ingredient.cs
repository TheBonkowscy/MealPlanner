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

    public static Result<Ingredient> Create(string name, List<MeasureUnit> applicableUnits)
    {
        var errors = new List<Error>();
        errors.AddRule(!string.IsNullOrWhiteSpace(name), DomainErrors.Ingredients.InvalidName);
        errors.AddRule(applicableUnits.Count > 0, DomainErrors.Ingredients.MissingMeasureUnits);

        return errors.Count > 0 ? Result.Failure<Ingredient>(errors) : Result.Success(new Ingredient(name, applicableUnits));
    }

    public bool IsApplicableUnit(MeasureUnit unit) => ApplicableUnits.Any(x => x == unit);

    public Result UpdateApplicableUnits(List<MeasureUnit> applicableUnits)
    {
        if (applicableUnits.Count > 0)
        {
            return Result.Failure(DomainErrors.Ingredients.MissingMeasureUnits);
        }
        
        ApplicableUnits = applicableUnits;
        return Result.Success();
    }
}