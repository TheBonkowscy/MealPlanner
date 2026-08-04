using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Meals;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Meals.Read;

public interface IReadMeals
{
    Task<GetMealsResponse> GetByQuery(string? query, CancellationToken cancellationToken = default);
}
public class MealsReader(MealPlannerDbContext ctx) : IReadMeals
{
    public async Task<GetMealsResponse> GetByQuery(string? query, CancellationToken cancellationToken = default)
    {
        IQueryable<Meal> dbQuery = ctx.Meals;
        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(query.ToLower()));
        }

        var result = await dbQuery.ToListAsync(cancellationToken);
        return new GetMealsResponse(result.Select(x => new MealListItemResponse(x.Id, x.Name)));
    }
}