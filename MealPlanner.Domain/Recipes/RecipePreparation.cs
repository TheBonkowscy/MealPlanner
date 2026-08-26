using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Recipes;

public class RecipePreparation
{
    public int Id { get; private set; }
    public string Description { get; private set; }
    public int LeadDays { get; private set; }
    public bool Required { get; private set; }

    private RecipePreparation()
    {
        // For EF Core
    }

    private RecipePreparation(string description, int leadDays, bool required)
    {
        Description = description;
        LeadDays = leadDays;
        Required = required;
    }

    public static Result<RecipePreparation> Create(string description, int leadDays, bool required)
    {
        var errors = new List<Error>()
            .AddRule(ValidateDescription(description), DomainErrors.RecipePreparation.InvalidDescription(description))
            .AddRule(ValidateLeadDays(leadDays), DomainErrors.RecipePreparation.InvalidLeadDays(leadDays));

        return errors.Count != 0 ? Result.Failure<RecipePreparation>(errors) : Result.Success(new RecipePreparation(description, leadDays, required));
    }

    private static bool ValidateDescription(string description)
    {
        return !string.IsNullOrWhiteSpace(description);
    }

    private static bool ValidateLeadDays(int leadDays)
    {
        return leadDays >= 1;
    }

    public Result UpdateDescription(string newDescription)
    {
        if (!ValidateDescription(newDescription))
        {
            return Result.Failure(DomainErrors.RecipePreparation.InvalidDescription(newDescription));
        }

        Description = newDescription;
        return Result.Success();
    }

    public Result UpdateLeadDays(int newLeadDays)
    {
        if (!ValidateLeadDays(newLeadDays))
        {
            return Result.Failure(DomainErrors.RecipePreparation.InvalidLeadDays(newLeadDays));
        }
        
        LeadDays = newLeadDays;
        return Result.Success();
    }

    public void UpdateRequired(bool required) => Required = required;
}