using MealPlanner.Domain.Shared;
using MealPlanner.Services.Shared;

namespace MealPlanner.Services.Shopping.Validators;

public static class DateRangeValidator
{
    public static Result Validate(DateOnly from, DateOnly to) =>
        to < from
            ? Result.Failure(ServiceErrors.Shopping.InvertedDateRange(from, to))
            : Result.Success();
}
