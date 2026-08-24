namespace MealPlanner.Domain.Shared;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<Error> Errors { get; }

    public Error Error => Errors.FirstOrDefault() ?? Error.None;

    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList().AsReadOnly();
    }

    public static Result Success() => new(true, Array.Empty<Error>());
    public static Result Failure(Error error) => new(false, new[] { error });
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Array.Empty<Error>());
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, new[] { error });
    public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => new(default, false, errors);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Can not retrieve value from a failure!");

    protected internal Result(TValue? value, bool isSuccess, IEnumerable<Error> errors) 
        : base(isSuccess, errors)
    {
        _value = value;
    }
}

public static class ResultBuilderExtensions
{
    public static List<Error> AddRule(
        this List<Error> errors, 
        bool condition, 
        Error error)
    {
        if (!condition)
        {
            errors.Add(error);
        }
        return errors;
    }

    public static List<Error> AllErrors(this IEnumerable<Result> results) => [.. results.Where(x => x.IsFailure).SelectMany(x => x.Errors)];
}