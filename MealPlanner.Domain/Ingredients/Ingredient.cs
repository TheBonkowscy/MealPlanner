using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Ingredients;

public class Ingredient
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }
    
    public int? ShelfLifeDays { get; private set; }

    public List<MeasureUnit> ApplicableUnits { get; private set; } = [];

    private Ingredient()
    {
        // For EF Core
    }
    
    private Ingredient(string name, int? shelfLifeDays, List<MeasureUnit> applicableUnits)
    {
        Name = name;
        ShelfLifeDays = shelfLifeDays;
        ApplicableUnits = applicableUnits;
    }

    public static Result<Ingredient> Create(string name, List<MeasureUnit> applicableUnits)
    {
        var errors = new List<Error>();
        errors.AddRule(!string.IsNullOrWhiteSpace(name), DomainErrors.Ingredient.InvalidName(name))
            .AddRule(applicableUnits.Count > 0, DomainErrors.Ingredient.MissingMeasureUnits(name));

        return errors.Count > 0 ? Result.Failure<Ingredient>(errors) : Result.Success(new Ingredient(name, null, applicableUnits));
    }

    public static Result<Ingredient> Create(string name, int shelfLife, List<MeasureUnit> applicableUnits)
    {
        var errors = new List<Error>();
        errors.AddRule(!string.IsNullOrWhiteSpace(name), DomainErrors.Ingredient.InvalidName(name))
            .AddRule(applicableUnits.Count > 0, DomainErrors.Ingredient.MissingMeasureUnits(name))
            .AddRule(shelfLife >= 0, DomainErrors.Ingredient.InvalidShelfLife(name));

        return errors.Count > 0 ? Result.Failure<Ingredient>(errors) : Result.Success(new Ingredient(name, shelfLife, applicableUnits));
    }

    public bool IsApplicableUnit(MeasureUnit unit) => ApplicableUnits.Any(x => x == unit);

    internal Result UpdateApplicableUnits(List<MeasureUnit> applicableUnits)
    {
        if (applicableUnits.Count <= 0)
        {
            return Result.Failure(DomainErrors.Ingredient.MissingMeasureUnits(Name));
        }
        
        ApplicableUnits = applicableUnits;
        return Result.Success();
    }

    public Result UpdateShelfLife(int? newShelfLifeDays)
    {
        if (newShelfLifeDays is < 0)
        {
            return Result.Failure(DomainErrors.Ingredient.InvalidShelfLife(Name));
        }

        ShelfLifeDays = newShelfLifeDays;
        return Result.Success();
    }
}