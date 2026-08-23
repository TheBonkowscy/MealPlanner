namespace MealPlanner.Domain;

public sealed record Error(
    string Code,
    ErrorType Type,
    IReadOnlyDictionary<string, object?> Metadata)
{
    public static Error Validation(string code, IReadOnlyDictionary<string, object?> metadata) =>
        new(code, ErrorType.Validation, metadata);

    public static Error NotFound(string code, IReadOnlyDictionary<string, object?> metadata) =>
        new(code, ErrorType.NotFound, metadata);

    public static Error Conflict(string code, IReadOnlyDictionary<string, object?> metadata) =>
        new(code, ErrorType.Conflict, metadata);

    public static readonly Error None = new("", ErrorType.None, new Dictionary<string, object?>());
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