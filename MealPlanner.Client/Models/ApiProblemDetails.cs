namespace MealPlanner.Client.Models;

public class ApiProblemDetails
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public int Status { get; set; }

    public List<ApiProblemError> Errors { get; set; } = [];
}

public class ApiProblemError
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";

    public Dictionary<string, object?> Metadata { get; set; } = [];
}