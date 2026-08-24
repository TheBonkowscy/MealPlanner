namespace MealPlanner.Client.Models;

public class ApiResult
{
    public bool IsSuccess { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResult Success() => new() { IsSuccess = true };
    public static ApiResult Failure(ApiError error) => new() { Error = error };
}

public class ApiResult<T> : ApiResult
{
    public T? Value { get; init; }

    public static ApiResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static new ApiResult<T> Failure(ApiError error) =>
        new() { Error = error };
}

public record ApiError(
    int Status,
    string Code,
    string Message,
    Dictionary<string, object?> Metadata);