using MealPlanner.Domain.Shared;
using MealPlanner.Services.Shared;
using MealPlanner.Services.Shopping.Validators;

namespace MealPlanner.Services.Shopping;

public interface IClosedDaysProvider
{
    Task<Result<List<DateOnly>>> GetClosedDays(DateOnly from, DateOnly to, CancellationToken ct);
}

public class StaticClosedDaysProvider(params DateOnly[] closedDays) : IClosedDaysProvider
{
    public Task<Result<List<DateOnly>>> GetClosedDays(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var validationResult = DateRangeValidator.Validate(from, to);
        return Task.FromResult(validationResult.IsFailure 
            ? Result.Failure<List<DateOnly>>(validationResult.Errors) 
            : Result.Success(closedDays.Where(d => from <= d && d <= to).ToList()));
    }
}