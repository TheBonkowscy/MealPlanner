using MealPlanner.Services.DailyMenus.Create;
using MealPlanner.Services.DailyMenus.Read;

namespace MealPlanner.API.DailyMenus;

public static class Extensions
{
    public static Task UseDailyMenuEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(Create.Endpoint.Address, Create.Endpoint.Create)
            .WithDisplayName("Create Daily Menu")
            .WithTags(Constants.OpenApiGroupName);
        
        app.MapGet(Read.ByIdEndpoint.Address, Read.ByIdEndpoint.Read)
            .WithDisplayName("Read Daily Menu")
            .WithTags(Constants.OpenApiGroupName);

        app.MapGet(Read.ForTodayEndpoint.Address, Read.ForTodayEndpoint.Read)
            .WithDisplayName("Read Daily Menu for Today")
            .WithTags(Constants.OpenApiGroupName);
        
        app.MapGet(Read.ForSpecificDateEndpoint.Address, Read.ForSpecificDateEndpoint.Read)
            .WithDisplayName("Read Daily Menu for Specific Date")
            .WithTags(Constants.OpenApiGroupName);

        return Task.CompletedTask;
    }

    public static Task RegisterDailyMenuServices(this IServiceCollection services)
    {
        services.AddTransient<ICreateDailyMenu, DailyMenuCreator>();
        services.AddTransient<IReadDailyMenu, DailyMenuReader>();
        return Task.CompletedTask;
    }
}