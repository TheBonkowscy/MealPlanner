namespace MealPlanner.Domain;

public sealed record Error(
    string Code,
    ErrorType Type)
{
    public static Error Validation(string code) => new(code, ErrorType.Validation);

    public static Error NotFound(string code) => new(code, ErrorType.NotFound);

    public static Error Conflict(string code) => new(code, ErrorType.Conflict);

    public static readonly Error None = new("", ErrorType.None);
}

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure
}