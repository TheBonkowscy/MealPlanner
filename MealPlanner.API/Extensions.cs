using MealPlanner.Domain.Shared;
using MealPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API;

public static class Extensions
{
    extension(Result result)
    {
        public IResult ToHttpResult(IStringLocalizer<Translations> localizer)
        {
            return result.IsSuccess ? Results.NoContent() : result.CreateProblem(localizer);
        }

        private IResult CreateProblem(IStringLocalizer<Translations> localizer)
        {
            var first = result.Error;

            var problem = new ProblemDetails
            {
                Status = MapStatus(first.Type),
                Type = first.Code,
                Title = localizer[first.Code, GetArguments(first)]
            };

            problem.Extensions["errors"] = result.Errors.Select(error => new
            {
                Code = error.Code,
                Message = localizer[error.Code, GetArguments(error)].Value,
                Metadata = error.Metadata
            }).ToList();

            return Results.Problem(problem);
        }

        private static int MapStatus(ErrorType type) => type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        private static object[] GetArguments(Error error) =>
        [
            ..error.Metadata.Values.Select(FormatValue) ?? []
        ];

        private static object? FormatValue(object? value) => value switch
        {
            IEnumerable<int> integers => string.Join(", ", integers),
            IEnumerable<Guid> ids => string.Join(", ", ids),
            _ => value
        };
    }

    extension<T>(Result<T> result)
    {
        public IResult ToHttpResult(IStringLocalizer<Translations> localizer)
        {
            return result.IsSuccess ? Results.Ok(result.Value) : result.CreateProblem(localizer);
        }
    }
}