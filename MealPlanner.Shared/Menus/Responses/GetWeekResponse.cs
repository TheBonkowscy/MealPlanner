namespace MealPlanner.Shared.Menus.Responses;

public record GetWeekResponse(List<GetMenuResponse> Days, DateOnly WeekStart)
{
    public static GetWeekResponse Empty => new GetWeekResponse([], DateOnly.MinValue);
}