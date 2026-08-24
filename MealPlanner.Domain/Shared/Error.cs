namespace MealPlanner.Domain.Shared;

public sealed record Error(
    string Code,
    ErrorType Type,
    IReadOnlyDictionary<string, object?> Metadata)
{
    public static Error Validation(string code, params (string Key, object?Value)[] metadata) => new(code,
        ErrorType.Validation, metadata.ToDictionary(x => x.Key, x => x.Value));

    public static Error NotFound(string code, params (string Key, object?Value)[] metadata) =>
        new(code, ErrorType.NotFound, metadata.ToDictionary(x => x.Key, x => x.Value));

    public static Error Conflict(string code, params (string Key, object?Value)[] metadata) =>
        new(code, ErrorType.Conflict, metadata.ToDictionary(x => x.Key, x => x.Value));

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