using MealPlanner.Services.Meals;
using MealPlanner.Services.Meals.Read;
using MealPlanner.Services.Menus;

namespace MealPlanner.API.Menus;

public static class Extensions
{
    public static Task RegisterMenuServices(this IServiceCollection services)
    {
        services.AddTransient<ICreateMenu, MenuCreator>();
        services.AddTransient<IReadMenu, MenuReader>();
        services.AddTransient<IReadMeals, MealsReader>();
        services.AddTransient<IUpdateMenu, MenuUpdater>();
        services.AddTransient<IMapMeals, MealMapper>();
        return Task.CompletedTask;
    }
}