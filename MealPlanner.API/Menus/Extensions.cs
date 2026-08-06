using MealPlanner.Services.Meals;
using MealPlanner.Services.Meals.Read;
using MealPlanner.Services.Menus;

namespace MealPlanner.API.Menus;

public static class Extensions
{
    public static Task RegisterMenuServices(this IServiceCollection services)
    {
        /* Menus */
        services.AddTransient<ICreateMenu, MenuCreator>();
        services.AddTransient<IReadMenu, MenuReader>();
        services.AddTransient<IUpdateMenu, MenuUpdater>();
        services.AddTransient<IDeleteMenu, MenuDeleter>();
        
        /* Meals */
        services.AddTransient<IReadMeals, MealsReader>();
        services.AddTransient<IMapMeals, MealMapper>();
        
        return Task.CompletedTask;
    }
}