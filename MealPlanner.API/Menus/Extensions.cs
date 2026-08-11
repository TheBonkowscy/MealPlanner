using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Read;

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
        
        /* Meals & Recipes */
        services.AddTransient<IReadRecipes, RecipesReader>();
        services.AddTransient<IMapRecipes, RecipeMapper>();
        
        return Task.CompletedTask;
    }
}